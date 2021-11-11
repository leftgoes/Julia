import cv2
from inspect import getsource
from multiprocessing import Pool
import numpy as np
import os
import time
from typing import Callable

from objects import CalculateMandelbrot, CalculateJulia, TestThreading, NoDataException
import functions

from collections import Counter


class Julia:
    def __init__(self, f: Callable, c: complex, width: int = 640, real: tuple[float, float] = (-2, 2), imag: tuple[float, float] = (-2, 2), max_iterations: int = 30, extra_iterations: int = 2, max_magnitude: float = 2, threads: int = 8, info: bool = True, exponent: int = 2, oversample: int = 1, square_tiling: bool = True, check: bool = False):
        self.f = f
        self.c = c
        self.w = width
        self.real = real
        self.imag = imag

        self.max_iterations = max_iterations
        self.max_magnitude = max_magnitude
        self.extra_iterations = extra_iterations

        self.info = info
        self.threads = threads
        self.exponent = exponent
        self.oversample = oversample
        self.tiling = square_tiling

        self.arr, self.delta, self.elapsed, self.method, self.checked, self.render_area, self.render_areas, self.squares = (None for _ in range(8))
        self.check = (self.w + self.h()) / 2 < 3000 and check

    def __str__(self):
        z_range = f'({complex(self.real[0], self.imag[0])}, {complex(self.real[1], self.imag[1])})'.replace('(', '').replace(')', '').replace('j', 'i')
        return f'{self.type}; ({z_range}); i({self.max_iterations}, {self.extra_iterations}); img({self.w}, {self.h() + abs(self.h_mirrored)}); r{self.max_magnitude}; th{self.threads}; t{round(self.elapsed, 2)}; o{self.oversample}'

    def __repr__(self):
        z_range = f'({complex(self.real[0], self.imag[0])}, {complex(self.real[1], self.imag[1])})'.replace('(', '').replace(')', '').replace('j', 'i')
        return f'{self.type}; ({z_range}); ({self.w}, {self.h() + abs(self.h_mirrored)}; {self.max_iterations}); {self.oversample}'

    @staticmethod
    def diff(tup: tuple) -> float or int:
        if type(tup) is not tuple:
            raise TypeError
        return abs(tup[1] - tup[0])

    @staticmethod
    def is_pow2(n) -> bool:
        '''https://stackoverflow.com/questions/57025836/how-to-check-if-a-given-number-is-a-power-of-two'''
        return (n & (n - 1) == 0) and n != 0

    @staticmethod
    def round(num: float, digits: int) -> str:
        num = round(num, digits)
        string = str(num)
        return f'{num}{"0" * (digits - len(string[string.find("."):]) + 1)}'

    def h(self, w: int = None, real: tuple = None, imag: tuple = None) -> int:
        w = self.w if w is None else w
        if real is None:
            real = self.real
        if imag is None:
            imag = self.imag
        return round(w * abs(self.diff(imag) / self.diff(real)))

    @property
    def flip(self) -> tuple[bool, bool]:
        (re1, re2), (im1, im2) = self.real, self.imag
        return abs(re2) > abs(re1), abs(im1) > abs(im2)

    def get_render_areas(self) -> None:
        self.sort()
        re_1, re_2 = self.real
        im_1, im_2 = self.imag
        im_long = max(abs(im_1), abs(im_2))
        if np.sign(im_1) != np.sign(im_2) and np.sign(re_1) != np.sign(re_2):  # cannot exploit symmetry
            imag = (im_long, 0)
            if re_1 == -re_2:
                self.render_areas = [(self.real, imag, 0, (self.w, self.h(imag=imag)))]
            else:
                real_2, imag_2 = (re_1, -re_2), (0, -min(abs(im_1), abs(im_2)))
                h_1 = self.h(imag=imag)
                w_2 = round(self.w * self.diff(real_2) / self.diff(self.real))
                self.render_areas = [(self.real, imag, 0, (self.w, h_1)), (real_2, imag_2, h_1, (w_2, self.h(w=w_2, real=real_2, imag=imag_2)))]
        else:
            self.render_areas = [(self.real, self.imag, 0, (self.w, self.h()))]
        self.sort()

    @property
    def type(self) -> str:
        lines = getsource(self.f).splitlines()
        for line in lines:
            if line.find('return') != -1:
                text = f"{lines[0][4:-1]} = {line.replace('    ', '')[7:].replace('**', '^').replace('*', '·')}"
                return f'Julia; {text.replace(", c", "")}'
        return 'Julia'

    def symmetry(self, arrays: list[np.ndarray, ...]) -> np.ndarray:
        main_arr = arrays[0][0]
        if len(arrays) == 1:
            if self.real[0] == -self.real[1]:
                return np.concatenate((main_arr, main_arr[::-1, ::-1]))
            else:
                return main_arr
        else:
            arr = np.zeros((self.h(), self.w))
            for sub_arr, y_offset in arrays:
                h, w = sub_arr.shape
                arr[y_offset:y_offset + h, :w] = sub_arr
            w2, h2 = arrays[1][0].shape
            arr[y_offset:, w2:] = main_arr[::-1, ::-1][:h2, :self.w - w2]
            return arr

    def sort(self) -> None:
        real, imag = self.real, self.imag
        self.real = real if real[1] > real[0] else real[::-1]
        self.imag = imag if imag[1] > imag[0] else imag[::-1]

    def continuous(self, z: complex, i: int) -> float:
        if i == 0:
            return 0.0
        else:
            continuous_i = i + 0.5 - np.log(np.log(abs(z))) / np.log(self.exponent)
            return 0.0 if continuous_i <= 0 else continuous_i

    def get_squares(self, w: int, h: int, x0: int = 0, y0: int = 0, size_offset: int = None) -> list[tuple[tuple[int, int], int], ...]:
        if w == 0 or h == 0:
            return []
        elif w == h == 1:
            return [((x0, y0), 1)]
        else:
            if size_offset is None:
                size_offset = 0
            square_size = 2 ** (int(np.log2(min(w, h))) - size_offset)
            w_mod, h_mod = w % square_size, h % square_size

            squares = []
            for x in range(w // square_size):
                for y in range(h // square_size):
                    squares.append(((x * square_size + x0, y * square_size + y0), square_size))

            squares += self.get_squares(w_mod, h, w - w_mod + x0, y0)  # vertical on right
            squares += self.get_squares(w - w_mod, h_mod, x0, h - h_mod + y0)  # horizontal at bottom
            return squares

    def calculate_pixel(self, z: complex) -> tuple[complex, int]:
        for i in range(self.max_iterations):
            z = self.f(z=z, c=self.c)
            if abs(z) > self.max_magnitude:
                for _ in range(self.extra_iterations):
                    z = self.f(z=z, c=self.c)
                return z, i
        return z, 0

    def calculate_square(self, zeros: set, nonzeros: dict, x0: int, y0: int, size: int) -> tuple[dict, list]:
        # get Δx, Δy
        delta_x, delta_y = self.delta
        real, imag, _, shape = self.render_area
        if size == 1:
            if (x0, y0) in nonzeros or (x0, y0) in zeros:
                return {}, []
            else:
                z = functions.xy2complex(x0 + delta_x, y0 + delta_y, real, imag, shape)
                z, i = self.calculate_pixel(z)
                return ({}, [((x0, y0), 1)]) if i == 0 else ({(x0, y0): self.continuous(z, i)}, [((x0, y0), 1)])

        zeros, nonzeros, checked = set(), {}, []

        right = [(x0 + size - 1, y0 + i) for i in range(1, size)]
        top = [(x0 + i, y0) for i in range(1, size)]
        left = [(x0, y0 + i) for i in range(size - 1)]
        bottom = [(x0 + i, y0 + size - 1) for i in range(size - 1)]

        for x, y in left + right + top + bottom:
            z = functions.xy2complex(x + delta_x, y + delta_y, real, imag, shape)
            z, iterations = self.calculate_pixel(z)
            if iterations == 0:
                zeros.update((x, y))
            else:
                if self.check:
                    checked.append(((x0, y0), size))
                nonzeros.update({(x, y): self.continuous(z, iterations)})
                break
        else:
            return nonzeros, checked

        new_size = size // 2
        northwest = self.calculate_square(zeros, nonzeros, x0, y0, new_size)
        northeast = self.calculate_square(zeros, nonzeros, x0 + new_size, y0, new_size)
        southwest = self.calculate_square(zeros, nonzeros, x0, y0 + new_size, new_size)
        southeast = self.calculate_square(zeros, nonzeros, x0 + new_size, y0 + new_size, new_size)
        for q_nonzeros, q_checked in [northwest, northeast, southwest, southeast]:
            nonzeros.update(q_nonzeros)
            checked += q_checked
        return nonzeros, checked

    def thread_tiling(self, n: int) -> tuple[np.ndarray, list]:  # thread with square_tiling enabled
        delta_x, delta_y = self.delta
        delta_progressbar = 8 + len(str(self.threads))
        progress_delta, check_n = delta_x + delta_y / self.oversample, self.threads // 2

        one = sum(sum(s[1]**2 for s in squares[n::self.threads]) for squares in self.squares)
        progress, checked, s = 0, [], time.time()

        arrays = []
        areas_num = len(self.render_areas)
        for j, (area, squares) in enumerate(zip(self.render_areas, self.squares)):
            self.render_area = area
            y_offset, (w, h) = area[2:]

            squares = squares[n::self.threads]
            sub_arr = np.zeros((h, w))
            for (x0, y0), sidelength in squares:
                pixels, s_checked = self.calculate_square(set(), dict(), x0, y0, sidelength)  # get all pixels that are not in set
                progress += sidelength ** 2
                checked += s_checked
                for (x, y), i in pixels.items():
                    sub_arr[y, x] = i
                if self.info and n == check_n:
                    progress_thread = j / areas_num
                    progress_area = progress / one
                    progress_total = progress_delta + (progress_thread + progress_area / areas_num) / self.oversample ** 2
                    print(f'\r[INFO] calculate | {functions.progressbar(progress_total, delta_progressbar)} | {functions.progressbar(progress_area)} | {self.round(100 * progress_area, 1)}% {self.round(time.time() - s, 2)}s {self.round(progress_area * (time.time() - s) / one, 2)}s', end='')
            arrays.append((sub_arr, y_offset))
        progress_total = delta_x + (delta_y + 1/self.oversample)/self.oversample

        arr = self.symmetry(arrays)
        if self.info and n == check_n:
            print(f'\r[INFO] calculate | {functions.progressbar(progress_total, delta_progressbar)} | waiting for other threads..{60*" "}', end='')
        return arr, checked

    def calculate(self, size_offset: int = 0) -> float:
        self.checked = []

        self.get_render_areas()
        self.squares = []
        for area in self.render_areas:
            w, h = area[3]
            self.squares.append(self.get_squares(w, h, size_offset=2))

        if self.info:
            print(f'[INFO] calculate | {self.threads} threads | size = (w={self.w}, h={self.h() + abs(self.h_mirrored)}) | iterations = {self.max_iterations}')

        pool = Pool(self.threads)
        s = time.time()
        for delta_x in range(self.oversample):
            for delta_y in range(self.oversample):
                self.delta = (delta_x/self.oversample, delta_y/self.oversample)
                pool_map = pool.map(self.thread_tiling, range(self.threads))
                if self.arr is None:
                    self.arr = sum(t[0] for t in pool_map)
                else:
                    self.arr += sum(t[0] for t in pool_map)
                if self.info:
                    print(f'\r[INFO] calculate | {functions.progressbar(self.delta[0] + (self.delta[1] + 1/self.oversample)/self.oversample, 8 + len(str(self.threads)))} | adding arrays from threads' + 60*' ', end='')
                if self.check:
                    for t in pool_map:
                        self.checked += t[1]
        self.elapsed = time.time() - s
        pool.close()

        if self.info:
            print(f'\r[INFO] calculate | finished in {round(self.elapsed, 2)}s' + 60*' ')
        return self.elapsed

    def arr2img(self, arr: np.ndarray, depth: int, percentile: float) -> np.ndarray:
        if arr.max() == 0.0:
            raise NoDataException
        if percentile == 0.0:
            arr /= arr.max()
        else:
            arr = functions.asinh_stretch(arr, percentile)

        lr, ud = self.flip
        if lr:
            arr = np.fliplr(arr)
        if ud:
            arr = np.flipud(arr)
        return arr

    def show(self, percentile: float = 3.):
        arr = self.arr2img(self.arr, 8, percentile)
        arr *= 255/arr.max()
        cv2.imshow(repr(self), arr.astype(np.uint8))
        if self.info:
            print('[INFO] show')
        cv2.waitKey(0)

    def save(self, filename: str = None, depth: int = 16, percentile: float = 3., boxes: float = 0.):
        if filename is None:
            filename = str(self)
        dtype = np.uint8 if depth == 8 else np.uint16 if depth == 16 else None
        if dtype is None:
            raise ValueError(f"'depth' must be 8 or 16 not {depth}")

        img_arr = self.arr2img(self.arr, depth, percentile)
        if boxes == 0.:
            img_arr *= 2 ** depth - 1
        else:
            start = time.time()
            boxes_arr = functions.boxes2arr(self.w, self.h(), self.checked, self.info)
            if self.info:
                print(f'\r[INFO] boxes2arr | finished in {round(time.time() - start, 2)}s')
            boxes_arr = self.arr2img(boxes_arr, depth, 0.0)
            img_arr = (1 - boxes) * img_arr + boxes * boxes_arr
            img_arr *= 2 ** depth - 1
            filename += f'; b{round(100 * boxes)}'
        cv2.imwrite(f'{filename}.png', img_arr.astype(dtype))
        if self.info:
            print(f"[INFO] saved to '{filename}.png'")


class Mandelbrot(Julia):
    def __init__(self, f: Callable, *args, **kwargs):
        super().__init__(f, None, *args, **kwargs)

    @property
    def type(self) -> str:
        lines = getsource(self.f).splitlines()
        for line in lines:
            if line.find('return') != -1:
                text = f"{lines[0][4:-1]} = {line.replace('    ', '')[7:].replace('**', '^').replace('*', '·')}"
                return f"Mandelbrot; {text}"
        return 'Mandelbrot'

    def get_render_areas(self) -> None:
        self.sort()
        re_1, re_2 = self.real
        im_1, im_2 = self.imag
        im_long = max(abs(im_1), abs(im_2))
        if np.sign(im_1) != np.sign(im_2):  # cannot exploit symmetry
            imag = (im_long, 0)
            self.render_areas = [(self.real, imag, 0, (self.w, self.h(imag=imag)))]
        else:
            self.render_areas = [(self.real, self.imag, 0, (self.w, self.h()))]
        self.sort()

    def calculate_pixel(self, c: complex) -> tuple[complex, int]:
        z = 0
        for i in range(self.max_iterations):
            z = self.f(z=z, c=c)
            if abs(z) > self.max_magnitude:
                for _ in range(self.extra_iterations):
                    z = self.f(z=z, c=c)
                return z, i
        return z, 0

    def symmetry(self, arrays: list[np.ndarray]) -> np.ndarray:
        imag = self.render_areas[0][1]
        main_arr = arrays[0][0]
        if self.imag[0] == -self.imag[1]:
            return np.concatenate((main_arr, main_arr[::-1, ]))
        elif np.sign(self.imag[0]) + np.sign(self.imag[1]) == 0:
            h_mirrored = round(self.h() * min(self.imag, key=lambda i: abs(i))/self.diff(self.imag))
            return np.concatenate((main_arr, main_arr[::-1, ][:h_mirrored, ]))
        else:
            return main_arr


def f(z, c):
    return z ** 2 + c



if __name__ == '__main__':
    image = Julia(f, width=1200, mandelbrot=False, max_iterations=300, c=0.28+0.0075j, oversample=7)
    image.calculate()
    image.show(percentile=10)
    image.save(percentile=10)

