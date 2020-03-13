# BMPScript's PARSE Command
## Summary
The PARSE command takes a new bitmap for parsing. This way, one can recursively call up to 100 bitmaps deep. This can be a way to parse the output of your program as a new program, until you end up with only one black and one white pixel (entry and exit point).
## Usage
The first time the PARSE command is invoked, it searches (in the working directory) for a file called ``0.bmp``. The next time, it searches for ``1.bmp`` and so on.  
By default, the output of a program is stored in the same files as the PARSE command tries to read, except the index is one element higher (so ``1.bmp``, then ``2.bmp``). 
### Recursive calling of images
This is an example of how to use the PARSE command in a loop to parse an image ``0.bmp`` and reparse it until the program reaches ``100.bmp``.  
```
```
## Internally
The ``Jay.BMPScript.Parser`` class (in ``./Parser.cs``) uses three variables in order to use the PARSE command:  
 * ``private int Read``, the current file reading index,  
 * ``private int Writer``, the current file writing index,  
 * ``private int Depth``, the current depth (which is passed on to the Loader).  
