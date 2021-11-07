import numpy as np
from numpy import sinh, sqrt
from math import asinh


def xy2complex(x: float, y: float, real: tuple, imag: tuple, shape: tuple[int, int]) -> complex:
    return complex(linmap(x, (0, shape[0]), real), linmap(y, (0, shape[1]), imag))


def complex2yx(z: complex, real: tuple, imag: tuple, shape: tuple[int, int]) -> tuple[int, int]:
    return round(linmap(z.imag, imag, (0, shape[1]))), round(linmap(z.real, real, (0, shape[0])))


def linmap(values, actual_bounds, desired_bounds):
    return desired_bounds[0] + (values - actual_bounds[0]) * (desired_bounds[1] - desired_bounds[0]) / (actual_bounds[1] - actual_bounds[0])


def gray2color(array: np.ndarray, black2color: tuple, white2color: tuple):
    max_val = array.max()
    h, w = array.shape
    color_array = np.zeros((h, w, 3))
    for i, y in enumerate(array):
        for j, x in enumerate(y):
            if x != 0.0:
                bgr = [linmap(x, (0.0, max_val), (black2color[i], white2color[i])) for i in range(2, -1, -1)]
                for channel, val in zip(range(3), bgr):
                    color_array[i, j, channel] = val
    return color_array


def asinh_stretch(array: np.ndarray, percentile) -> np.ndarray:  # asinh/sinh stretch
    # scale to [0, 1]
    array = array - array.min()
    array /= array.max()
    # stretch
    median = np.median(np.percentile(array[np.nonzero(array)], 100 - percentile))
    if median != 0.5:
        if median < 0.5:
            a = sqrt(1 - 4 * median ** 2) / (2 * median ** 2)
            f = asinh
        else:
            a = sqrt(1 - 4 * (1 - median) ** 2) / (2 * (1 - median) ** 2)
            f = sinh
        f_a = f(a)
        for i, y in enumerate(array):
            for j, x in enumerate(y):
                array[i, j] = 0 if x == 0 else f(a * x) / f_a
    return array


def progressbar(progress: float, length: int = 30):
    quarters = '_░▒▓█'
    done = int(progress * length)
    return (done * '█' + quarters[round(4 * (length * progress - done))] + int((1 - progress) * length) * '_')[:length]


def boxes2arr(w: int, h: int, boxes: list, info: bool = False):
    a = np.zeros((h, w))
    length = len(boxes)
    for n, ((x, y), size) in enumerate(boxes):
        for i in range(size):
            for j in range(size):
                if i == 0 or j == 0 or i == size - 1 or j == size - 1:
                    a[y + i, x + j] += max(w, h) // 5
                a[y + i, x + j] += size
        if info:
            print(f'\r[INFO] boxes2arr | {progressbar(n/length)} | {round(100 * n/length, 2)}%\t', end='')
    return a


