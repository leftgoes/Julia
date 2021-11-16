import math
from multiprocessing import Pool
import random
import numpy as np
import cv2
import time

import functions


class Buddhabrot:
    def __init__(self, width: int, k: int, n: int, percentage: float = 0.5, threads: int = 8, info: bool = True):
        self.w = width
        self.k = min(k, n)
        self.n = max(k, n)
        self.real = (-2, 2)
        self.imag = (-2, 2)
        self.percentage = percentage
        self.threads = threads
        self.info = info

        self.arr: np.ndarray
        self.elapsed: float
        self.numbers: list[complex, ...]

    def __str__(self):
        z_range = f'({complex(self.real[0], self.imag[0])}, {complex(self.real[1], self.imag[1])})'.replace('(', '').replace(')', '').replace('j', 'i')
        return f'Buddhabrot; ({z_range}); img({self.w}, {self.h()}); k{self.k}, n{self.n}; th{self.threads}; t{round(self.elapsed, 2)}; p{round(self.percentage, 2)}'

    def __repr__(self):
        z_range = f'({complex(self.real[0], self.imag[0])}, {complex(self.real[1], self.imag[1])})'.replace('(', '').replace(')', '').replace('j', 'i')
        return f'Buddhabrot; ({z_range}); ({self.w}, {self.h()}); ({self.k}, {self.n}); {round(self.elapsed, 2)}; {round(self.percentage, 2)}'

    @staticmethod
    def f(z, c):
        return z ** 2 + c

    @staticmethod
    def diff(tup: tuple[float or int, float or int]) -> float or int:
        if type(tup) is not tuple:
            raise TypeError
        return abs(tup[1] - tup[0])

    @staticmethod
    def p_1(z: complex) -> bool:  # main cardioid
        abs_z = abs(z)
        z -= 0.25  # shifted 1/4
        c0 = z / (2 * abs(z))
        c = c0 - c0 ** 2
        return abs_z < abs(c)

    @staticmethod
    def p_2(z: complex) -> bool:  # main disk
        return abs(z + 1) < 0.25

    def h(self, w: int = None, real: tuple = None, imag: tuple = None) -> int:
        w = self.w if w is None else w
        real = self.real if real is None else real
        imag = self.imag if imag is None else imag
        return round(w * abs(self.diff(imag) / self.diff(real)))

    def thread(self, t: int) -> np.ndarray:  # deep iteration
        h = round(self.h())
        arr = np.zeros((h, self.w))

        len_numbers = len(self.numbers[t::self.threads])
        print_every = len_numbers // 1000
        for n, c in enumerate(self.numbers[t::self.threads]):
            z, points = 0, []
            for i in range(self.n):
                z = self.f(z, c)
                y, x = functions.complex2yx(z, self.real, self.imag, (self.w, h))
                if 0 <= x < self.w and 0 <= y < h:  # in view
                    points.append((y, x))
                else:
                    if self.k < i:  # k < i < n
                        for j, k in points:
                            arr[j, k] += 1
                    break
            if self.info and t == 0 and n % print_every == 0:
                print(f'\r[INFO] calculate | {round(100 * n / len_numbers, 1)}%', end='')
        if self.info and t == 0:
            print(f'\r[INFO] calculate | 100%')
        return arr

    def calculate(self) -> float:
        imag, h = (-2, 2), round(self.h())
        start = time.time()

        if self.info:
            print(f'[INFO] calculate | {self.threads} threads, percentage = {self.percentage}')

        self.numbers = []
        for _ in range(round(self.percentage * self.w * h)):
            while True:
                c = complex(random.uniform(*imag), random.uniform(*self.real))
                if not (self.p_2(c) or self.p_1(c)):  # in Mandelbrot set
                    self.numbers.append(c)
                    break

        if self.info:
            print(f'[INFO] calculate | {len(self.numbers)}/{self.w * self.h()} pixels')

        pool = Pool(self.threads)
        self.arr = sum(pool.map(self.thread, range(self.threads)))
        pool.close()

        self.elapsed = time.time() - start

        if self.info:
            print(f'[INFO] calculate | finished in {round(self.elapsed, 2)}s')
        return self.elapsed

    def normalize_arr(self, arr: np.ndarray, percentile: float) -> np.ndarray:
        if arr.max() == 0.0:
            raise NoDataException
        if percentile == 0.0:
            arr /= arr.max()
        else:
            arr = functions.asinh_stretch(arr, percentile)

        # arr += arr[::-1]
        return arr

    def show(self, percentile: float = 3.):
        arr = self.normalize_arr(self.arr, percentile)
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

        img_arr = (2 ** depth - 1) * self.normalize_arr(self.arr, percentile)
        cv2.imwrite(f'{filename}.png', img_arr.astype(dtype))
        if self.info:
            print(f"[INFO] saved to '{filename}.png'")


if __name__ == '__main__':
    buddhabrot = Buddhabrot(4000, 100000, 200000, 0.8)
    buddhabrot.calculate()
    buddhabrot.save()
