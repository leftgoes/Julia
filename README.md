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
