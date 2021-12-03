import cv2
from main import Julia
import math
import matplotlib.pyplot as plt
import numpy as np
import time
from typing import Callable
import os


class JuliaVideo:
    __slots__ = ('f_c', 'start', 'end', 'frames', 'julia')

    def __init__(self, start: complex, end: complex, *args, frames: int = 300, sin_exponent: int = 3, **kwargs):
        re1, im1 = start.real, start.imag
        re2, im2 = end.real, end.imag

        def _f(t: float) -> complex:
            return complex((re2 - re1) * (np.sin(np.pi / (2 * frames) * t)) ** sin_exponent + re1,
                           (im2 - im1) * (np.sin(np.pi / (2 * frames) * t)) ** sin_exponent + im1)

        self.end = end
        self.julia = Julia(*args, **kwargs)
        self.julia.info = False
        self.f_c = _f
        self.frames = frames
        self.start = start

    def get(self, file: str = None, extension: str = '.mp4', fps: int = 30, size_offset: int = 3, percentile: float = 3):
        start = time.perf_counter()

        print(f'[INFO] get | {self.julia.threads} threads | size = (w={self.julia.w}, h={self.julia.h()}) | iterations = {self.julia.max_iterations}')
        video = cv2.VideoWriter('__temp_video__' + extension, cv2.VideoWriter_fourcc(*'mp4v'), fps, (self.julia.h(), self.julia.w), False)
        for frame in range(self.frames + 1):
            self.julia.c = self.f_c(frame)
            self.julia.calculate(size_offset)
            print(f'\r[INFO] get | {round(100 * frame/self.frames, 2)}%', end='')
            arr = self.julia.normalize_arr(self.julia.arr, percentile)
            video.write((255 * arr/arr.max()).astype(np.uint8))
        video.release()
        if file is None:
            file = f'JuliaVideo; c({str(self.start).replace("j", "i")}, {str(self.end).replace("j", "i")}) ;z({self.julia.z_range}); i({self.julia.max_iterations}, {self.julia.extra_iterations}); img({self.julia.w}, {self.julia.h() + abs(self.julia.h_mirrored)}); r{self.julia.max_magnitude}; th{self.julia.threads}; t{round(time.perf_counter() - start, 2)}; o{self.julia.oversample}'
        os.rename('__temp_video__' + extension, file + extension)


def f(z, c):
    return z ** 2 + c


if __name__ == '__main__':
    julia_video = JuliaVideo(-0.76+0.2j, -0.75-0.06j, f, width=1080, max_iterations=500)
    julia_video.get()
