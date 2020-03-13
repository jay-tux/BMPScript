# 2D Snake Iteration
*A kind of IEnumerator/iterator for iterating over 2D arrays*  
The Snake IEnumerator is defined in ``./Iteration.cs`` as:  
```
public System.Collection.IEnumerator<Point> Snake(int Direction, bool OneRun = false)
```
The ``Direction`` parameter can be one of ``(0, 90, 180, 270)``, each defining a rotation. When given another rotation, the ``Iteration2D`` will throw a ``IEnumeratorException``. The ``OneRun`` parameter defines whether it should rotate infinitely (until interrupted) or until it reaches its starting point.  
The effect of the ``Direction`` parameter is illustrated with some images.  
**0 degrees**  

**90 degrees**  

**180 degrees**  

**270 degrees**  
