# Julia
- Julia sets
- Mandelbrot set
- Buddhabrot & Nebulabrot fractal
- Video of Julia sets for different c
  - flickering problem (all arrays are normalized to (0, 1))
  - depending on raw image brightness, ``asinh_stretch`` yields visually different results
  - possible fix is high oversampling factor but render time would be increased
- C# code for Buddhabrot
- C extension for ``f`` planned
  - dramatic performance improvement expected (C vs. Python) since ``f`` takes up the most time by far (followed by ``boxes2arr``)
- Images (scaled down)

## C#
Because Python is such a slow language and this code in particular is heaviliy dependent on speed because it is processor-intensive, implementations in C or even C# will be much faster. I have little experience in these languages however, so I have thus far only implemented the Buddhabrot in C#.
### ``p_1``, ``p_2`` and the ``do``-``while``-loop
The do-while-loop that rejects those complex numbers, that ly inside the main cardioid (period 1) or the main disk (period 2) has a bug which seems to move all pixels to the right by approximately ``0.5``. Doing a simple ``z[0] -= 0.5`` doesn't fix it though, giving up the do-while-loop completly and accepting the performance impact does however. I concluded, that ``p_1`` and ``p_2`` are correct, since ``testPeriods`` shows the main cardioid and disk unshifted, so further analysis is needed.

## Format
If not specified otherwise, the file name will be ``__str__`` and it will be saved as a 16-bit tif.

## Color
Currently I am colorizing the images with [GIMP](https://www.gimp.org/), I might implement [historgram coloring](https://en.wikipedia.org/wiki/Plotting_algorithms_for_the_Mandelbrot_set#Histogram_coloring) in the future.

## Images
![Julia; f(z) = z ^ 2 + c; (-2-2i, 2+2i); i(300, 2); img(1200, 1200); r2; th8; t62.04; o7](https://github.com/leftgoes/Julia/blob/main/images/J_002.jpg)
my profile picture

![Julia; f(z) = z ^ 2 + c; (-2+2i, -2+2i); i(400, 2); img(3000, 3000); r2; th8; t5.64](https://github.com/leftgoes/Julia/blob/main/images/J_001.jpg)

![Buddhabrot; (-2-2i, 2+2i); img(1500, 1500), k(50k, 500k, 50k, 5k), n(200k, 2m, 100k, 10k); th8; t](https://github.com/leftgoes/Julia/blob/main/images/B_001.jpg)

![Antibuddhabrot; f(z, c) = z ^ 2 + c; (-3-2i, 2+2i); 30; img(5000, 4000); r2; th1; t(73, 73)](https://github.com/leftgoes/Julia/blob/main/images/B_002.jpg)

![Mandelbrot; (-0.743352131+0.131366432i, -0.743352123+0.1313664416i); i(10000, 2); img(6000, -98524816908); r2; th8; t1895.96](https://github.com/leftgoes/Julia/blob/main/images/M_001.jpg)

![Mandelbrot; f(z, c) = z ^ 2 + c; (-0.74335212625-0.7433521253i, 0.1313664357+0.1313664364i); i(80000, 2); img(3000, 2211); r2; th8; t840.86](https://github.com/leftgoes/Julia/blob/main/images/M_002.jpg)

![Mandelbrot; f(z, c) = z ^ 3 + c; (-0.5-0.05i, 0.5+1.5i); i(2000, 2); img(12000, 21000); r2; th8; t3626.83; o3](https://github.com/leftgoes/Julia/blob/main/images/M_003.jpg)

![](https://github.com/leftgoes/Julia/blob/main/images/B_000.jpg)

Coloring method by [Melinda Green](https://superliminal.com/fractals/bbrot/)
