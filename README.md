# Julia
- Julia sets
- Mandelbrot set
- Buddhabrot & Nebulabrot fractal
- Video of Julia sets for different c
  - flickering problem (all arrays are normalized to (0, 1))
  - depending on raw image brightness, ``asinh_stretch`` yields visually different results
  - possible fix is high oversampling factor but render time would be increased
- C# code for Buddhabrot
  - unexpected result from the do-while-loop (creates random points/numbers)
  - everything seems to be shifted to the right by 0.5
  - ``testPeriods`` however yields expected result (which means ``p_1`` and ``p_2`` should be correct)
- C extension for ``f`` planned
  - dramatic performance improvement expected (C vs. Python) since ``f`` takes up the most time by far (followed by ``boxes2arr``)
## Format
If not specified otherwise, the file name will be ``__str__``
## Color
Currently I am colorizing the images with [GIMP](https://www.gimp.org/), I might implement [historgram coloring](https://en.wikipedia.org/wiki/Plotting_algorithms_for_the_Mandelbrot_set#Histogram_coloring) in the future.
## Images
![](https://github.com/leftgoes/Julia/blob/main/images/Buddhabrot%3B%20(-2-2i%2C%202%2B2i)%3B%20img(1500%2C%201500)%2C%20k(50k%2C%20500k%2C%2050k%2C%205k)%2C%20n(200k%2C%202m%2C%20100k%2C%2010k)%3B%20th8%3B%20t.jpg)
Buddhabrot, Composite of multiple renders

![](https://github.com/leftgoes/Julia/blob/main/images/Mandelbrot%3B%20(-0.743352131%2B0.131366432i%2C%20-0.743352123%2B0.1313664416i)%3B%20i(10000%2C%202)%3B%20img(6000%2C%20-98524816908)%3B%20r2%3B%20th8%3B%20t1895.96.jpg)
Mandelbrot, a minibrot

![](https://github.com/leftgoes/Julia/blob/main/images/Mandelbrot%3B%20f(z%2C%20c)%20%3D%20z%20%5E%202%20%2B%20c%3B%20(-0.74335212625-0.7433521253i%2C%200.1313664357%2B0.1313664364i)%3B%20i(80000%2C%202)%3B%20img(3000%2C%202211)%3B%20r2%3B%20th8%3B%20t840.86.jpg)
Mandelbrot, close-up of the minibrot above

![](https://github.com/leftgoes/Julia/blob/main/images/Mandelbrot%3B%20f(z%2C%20c)%20%3D%20z%20%5E%203%20%2B%20c%3B%20(-0.5-0.05i%2C%200.5%2B1.5i)%3B%20i(2000%2C%202)%3B%20img(12000%2C%2021000)%3B%20r2%3B%20th8%3B%20t3626.83%3B%20o3.jpg)
Mandelbrot, where ![](http://www.sciweavers.org/tex2img.php?eq=f%28z%2C%5C%3Ac%29%20%3D%20%20z%5E%7B3%7D%20%2B%20c&bc=Transparent&fc=White&im=png&fs=12&ff=modern&edit=0)
