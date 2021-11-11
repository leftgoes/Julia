import time
from numpy import array, linspace
from functions import progressbar
import matplotlib.pyplot as plt
from lmfit import Model
from inspect import getsource



class TestThreading:
    def __init__(self, obj, n: int, threads_range: tuple):
        self.obj = obj
        self.n = n
        self.threads_range = threads_range
        self.data, self.poly, self.coefficients, self.image = None, None, None, None
        self.elapsed = 0

    def calculate(self, *args, save: bool = False, **kwargs):
        t_range = self.threads_range[1] - self.threads_range[0]
        start = time.time()
        self.image = self.obj(*args, **kwargs, info=False)
        data = array([0.0 for _ in range(*self.threads_range)])
        print(f'\r{progressbar(0.)}\t0%', end='')
        for i in range(self.n):
            for j, k in enumerate(range(*self.threads_range)):
                self.image.array = None
                self.image.threads = k
                data[j] += self.image.calculate()
                if i == j == 0 and save:
                    self.image.save()
                progress = (i + (j + 1) / t_range) / self.n
                print(f'\r{progressbar(progress)}\t{round(100 * progress, 2)}%', end='')

        self.data = [range(*self.threads_range), data / self.n]
        self.elapsed = time.time() - start

    def approximate(self):
        if self.data is None:
            raise NoDataException(f"'{self.data = }' cannot be read")
        x, y = self.data
        args = [var[1] for var in self.poly_fit(list(x), y)[1:]]
        poly_x = linspace(*self.threads_range, 800)
        poly_y = self.polynomial(poly_x, *args)
        self.poly = (poly_x, poly_y)
        return args

    def show(self, save: bool = False):
        if self.data is None:
            raise NoDataException(f"'{self.data = }' cannot be read")
        x, y = self.data
        plt.plot(x, y, 'bo')
        if self.poly is not None:
            plt.plot(*self.poly, 'r--')
        plt.title(repr(self.image))
        plt.suptitle(f'multiprocessing; n = {self.n}; Σt = {round(self.elapsed, 2)}s')
        plt.xlabel('num_threads')
        plt.ylabel('elapsed time [s]')
        plt.xticks(x)
        if save:
            plt.savefig(str(time.time()) + '.png')
        plt.show()

    def poly_fit(self, x, y):
        for line in getsource(self.polynomial).splitlines():
            if line.find('def') != -1:
                args = line[line.find(',') + 2:line.find(')')].replace(' ', '').split(',')
                break
        else:
            raise Exception
        initial = dict(zip(args, [1 for _ in args]))
        model = Model(self.polynomial)
        return self.get_param(model.fit(y, x=x, **initial))

    @staticmethod
    def polynomial(x, b, c, d, e, f):
        return b / x ** 2 + c / x + d + e * x

    @staticmethod
    def get_param(result):
        parameters, report = [['variable', 'value', '+/-']], result.fit_report()
        for parm in report[report.find('[[Variables]]') + 18:report.find('[[Correlations]]')].replace('\n', '').split('    '):
            parm_data = [parm[0]]
            for p in parm.split(' '):
                try:
                    parm_data.append(float(p))
                except ValueError:
                    pass
            parameters.append(parm_data)
        return parameters

    def save(self):
        with open(str(self.image) + '.csv', 'w') as f:
            f.write(f'{self.data[0]}\n{self.data[1]}')
            f.write(f'\n{self.approximate()}')


class NoDataException(Exception):
    pass


def f(z, c):
    return z ** 2 + c


if __name__ == '__main__':
    threads = TestThreading(Julia, 2, (1, 12))
    threads.calculate(f, width=300, real=(0., 0.826), imag=(0., 0.928), max_iterations=10000, mandelbrot=True)
    threads.approximate()
    threads.show()
