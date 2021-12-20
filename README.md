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

