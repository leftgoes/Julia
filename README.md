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
- if not specified otherwise, the file name will be ``__str__``
## Images
![](https://github.com/leftgoes/Julia/blob/main/images/Buddhabrot%3B%20(-2-2i%2C%202%2B2i)%3B%20img(1500%2C%201500)%2C%20k(50k%2C%20500k%2C%2050k%2C%205k)%2C%20n(200k%2C%202m%2C%20100k%2C%2010k)%3B%20th8%3B%20t.jpg)
