# 2D Snake Iteration
*A kind of IEnumerator/iterator for iterating over 2D arrays*  
The Snake IEnumerator is defined in ``./Iteration.cs`` as:  
```
public System.Collection.IEnumerator<Point> Snake(int Direction, bool OneRun = false)
```
The ``Direction`` parameter can be one of ``(0, 90, 180, 270)``, each defining a rotation. When given another rotation, the ``Iteration2D`` will throw a ``IEnumeratorException``. The ``OneRun`` parameter defines whether it should rotate infinitely (until interrupted) or until it reaches its starting point.  
The effect of the ``Direction`` parameter is illustrated with some images.  
**0 degrees**  
![Starting at the upper left cell, then going to the right, turning down at the end of the first row.](https://github.com/jay-tux/BMPScript/blob/master/help/0.png  "0 degrees snake")  
**90 degrees**  
![Starting at the lower left cell, then going up, turning right at the top of the first column.](https://github.com/jay-tux/BMPScript/blob/master/help/90.png  "90 degrees snake")  
**180 degrees**  
![Starting at the lower right cell, then going to the left, turning up at the beginning of the last row.](https://github.com/jay-tux/BMPScript/blob/master/help/180.png  "180 degrees snake")  
**270 degrees**  
This is the default iteration over images for the BMPScript parser.  
![Starting at the upper right cell, then going down, turning left at the bottom of the last column.](https://github.com/jay-tux/BMPScript/blob/master/help/orderofexec.png  "270 degrees snake")  
