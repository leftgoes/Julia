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
## C#
Because Python is such a slow language and this code in particular is heaviliy dependent on speed because it is processor-intensive, implementations in C or even C# will be much faster. I have little experience in these languages however, so I have thus far only implemented the Buddhabrot in C#.
### ``p_1``, ``p_2`` and the ``do``-``while``-loop
The do-while-loop that rejects those complex numbers, that ly inside the main cardioid (period 1) or the main disk (period 2) has a bug which seems to move all pixels to the right by approximately ``0.5``. Doing a simple ``z[0] -= 0.5`` doesn't fix it though, giving up the do-while-loop completly and accepting the performance impact does however. I concluded, that ``p_1`` and ``p_2`` are correct, since ``testPeriods`` shows the main cardioid and disk unshifted, so further analysis is needed.

## Format
If not specified otherwise, the file name will be ``__str__``

## Color
Currently I am colorizing the images with [GIMP](https://www.gimp.org/), I might implement [historgram coloring](https://en.wikipedia.org/wiki/Plotting_algorithms_for_the_Mandelbrot_set#Histogram_coloring) in the future.

## Images
![](https://github.com/leftgoes/Julia/blob/main/images/Buddhabrot%3B%20(-2-2i%2C%202%2B2i)%3B%20img(1500%2C%201500)%2C%20k(50k%2C%20500k%2C%2050k%2C%205k)%2C%20n(200k%2C%202m%2C%20100k%2C%2010k)%3B%20th8%3B%20t.jpg)
Buddhabrot, Composite of multiple renders with different values for ``n`` and ``k``

![](https://github.com/leftgoes/Julia/blob/main/images/Mandelbrot%3B%20(-0.743352131%2B0.131366432i%2C%20-0.743352123%2B0.1313664416i)%3B%20i(10000%2C%202)%3B%20img(6000%2C%20-98524816908)%3B%20r2%3B%20th8%3B%20t1895.96.jpg)
Mandelbrot, a minibrot

![](https://github.com/leftgoes/Julia/blob/main/images/Mandelbrot%3B%20f(z%2C%20c)%20%3D%20z%20%5E%202%20%2B%20c%3B%20(-0.74335212625-0.7433521253i%2C%200.1313664357%2B0.1313664364i)%3B%20i(80000%2C%202)%3B%20img(3000%2C%202211)%3B%20r2%3B%20th8%3B%20t840.86.jpg)
Mandelbrot, close-up of the minibrot above

![](https://github.com/leftgoes/Julia/blob/main/images/Mandelbrot%3B%20f(z%2C%20c)%20%3D%20z%20%5E%203%20%2B%20c%3B%20(-0.5-0.05i%2C%200.5%2B1.5i)%3B%20i(2000%2C%202)%3B%20img(12000%2C%2021000)%3B%20r2%3B%20th8%3B%20t3626.83%3B%20o3.jpg)
Mandelbrot, for f(z, c) = z³ + c
