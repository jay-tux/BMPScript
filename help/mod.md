# Modifying Jay's BMPScript
## Structure
BMPScript uses two different C# ``namespace``s:
 * ``Jay.IEnumerators``, which can be used for iteration in non-linear ways and/or iteration over 2D or other kinds of datastructures (2 classes).
 * ``Jay.BMPScript``, which does the actual work for parsing and/or writing Bitmaps (8 classes).
## Class Overview
### Jay.IEnumerators.IEnumeratorException
File: ``./IEnumeratorException.cs``  
Inherits: ``System.Exception``  
*An Exception type which can occur while iterating using the ``Jay.IEnumerators`` library.*  
**Fields**  
``public string ErrorReason``  
A get-only string describing the error reason for the IEnumeratorException.
**Constructors**  
``public IEnumeratorException(string Message, string ErrorReason)``  
Creates an IEnumeratorException with the specified message and error reason.
**Methods**  

### Jay.IEnumerators.Iteration2D
File: ``./Iteration.cs``  
Inherits: none  
*A class supplying IEnumerators for iterating over 2D arrays (usually ``object[,]``), all represented by System.Drawing.Point structures.*  
**Fields**  
``private int _StartX``, ``private int _StartY``  
The current x and y positions of the pointer in the array.  
``private int _BoundX``, ``private int _BoundY``  
The maximal x and y dimensions of the array.  
``private int _RealStartX``, ``private int _RealStartY``  
The starting x and y positions of the pointer in the array.  
**Constructors**  
``public Iteration2D(int _StartX, int _StartY, int _BoundX, int _BoundY)``  
Creates a new Iteration2D for iterating over a ``_BoundX``x``_BoundY`` array, starting at (``_StartX``, ``_StartY``).  
**Methods**  
``public IEnumerator<Point> Linear()``  
Returns an IEnumerator which iterates linearly (line-by-line) over the provided 2D array.  
``public IEnumerator<Point> Reverse()``  
Returns an IEnumerator which iterates reversely (backwards line-by-line, bottom-to-top) over the provided 2D array.  
``public IEnumerator<Point> TTB_LTR()``  
Alias for ``Linear()``.  
``public IEnumerator<Point> BTT_RTL()``  
Alias for ``Reverse()``.  
``public IEnumerator<Point> BTT_LTR()``  
Returns an IEnumerator which iterates left-to-right over each line, starting at the bottom line, ending at the top.  
``public IEnumerator<Point> TTB_RTL()``  
Returns an IEnumerator which iterates right-to-left over each line, starting at the top line, ending at the bottom.  
``public IEnumerator<Point> Random()``  
Returns an IEnumerator which picks random elements from the 2D array.  
``public IEnumerator<Point> Snake(int direction, bool oneRun = false)``  
Returns a so-called Snakewise enumerator. For a complete overview of the Snake IEnumerator, see [help/snake.md](https://github.com/jay-tux/BMPScript/blob/master/help/snake.md).
***
### Jay.BMPScript.Program
File: ``./Program.cs``  
Inherits: none  
*The main entry point for the BMPScript parser.*  
**Fields**  
**Constructors**  
**Methods**  
``public static void Main(string[] args)``  
The entry point for the application. Tries start the Loader (which invokes the Parser) on the file ``args[0]``, otherwise, ``0.bmp`` in the current working directory.

### Jay.BMPScript.CodeChar
File: ``./CodeChar.cs``  
Inherits: none  
*A small datastructure representing one pixel (or one entity) of the bitmap (or program)*  
**Fields**
``private int R``, ``private int G``, ``private int B``  
Represents the red, green or blue component of one pixel.  
**Constructors**  
``private CodeChar(int R, int G, int B)``  
Creates a new program entity with the given red, green and blue components.  
**Methods**  
``public override string ToString()``
Converts a CodeChar to its (hexadecimal) string representation.
**Enums**
``public enum Order``  
Represents the possible commands.  
``public enum Part``  
Represents one part (one component) of a given pixel.  
**Implicit Cast Operators**  
``System.Drawing.Color`` to ``CodeChar`` ![Obsolete](https://img.shields.io/badge/%20-Obsolete-inactive)  
**Explicit Cast Operators**  
``CodeChar`` to ``int``,  
``byte[]`` to ``CodeChar``,  
``CodeChar`` to ``System.Drawing.Color``, ![Obsolete](https://img.shields.io/badge/%20-Obsolete-inactive)  
``CodeChar`` to ``char``, ![Obsolete](https://img.shields.io/badge/%20-Obsolete-inactive)  
``CodeChar`` to ``string``, and  
``CodeChar`` to ``CodeChar.Order``.  

### Jay.BMPScript.ConversionHelper ![Obsolete](https://img.shields.io/badge/%20-Obsolete-inactive)
File: ``./CodeChar.cs``
Inherits: none
*A static class which extends the ``System.Drawing.Color[]`` and ``System.Drawing.Color[,]`` functionality.*
**Fields**  
**Constructors**  
**Methods**  
**Extension Methods**  
``public static CodeChar[] ToCodeChar(this Color[] vl)`` ![Obsolete](https://img.shields.io/badge/%20-Obsolete-inactive)  
Converts a ``System.Drawing.Color[]`` to a ``CodeChar[]``.  
``public static CodeChar[,] ToCodeChar(this Color[,] vl)`` ![Obsolete](https://img.shields.io/badge/%20-Obsolete-inactive)  
Converts a ``System.Drawing.Color[,]`` to a ``CodeChar[,]``.  

### Jay.BMPScript.BMPScriptException
File: ``./BMPScriptException.cs``  
Inherits: ``System.Exception``  
*An Exception type which can occur while parsing a Bitmap.*  
**Fields**  
``public string Module``  
A string describing the module in which the BMPScriptException occured.  
``public string ExceptionType``  
A string containing a more specific description of the BMPScriptException's type.  
**Constructors**
``public BMPScriptException(string Module, string ExceptionType, string Message)``  
Creates a BMPScriptException with the specified module, exception type and message.  
**Methods**  

### Jay.BMPScript.ParseBMP
File: ``./ParseBMP.cs``  
Inherits: none  
*A class which is used to parse a bmp file into a ``CodeChar[,]`` structure.*  
**Fields**  
**Constructors**  
``public ParseBMP()``  
Creates a new BMP parser object.  
**Methods**
``protected int GetField(int byteOffset, int len, byte[] array)``  
Returns the integer representation of ``len`` bytes in ``array``, starting at ``byteOffset``.  
``public byte[] GetBytes(string BMP, out int Width, out int Height)``  
Opens the file ``BMP``, and read the image's width, height and color grid (color array).  
``public CodeChar[,] Recreate(string BMP)``  
Attempts to convert the bitmap image in ``BMP`` into its program entity equivalent.  

### Jay.BMPScript.Loader
File: ``./Loader.cs``  
Inherits: none  
*A class for loading the program from a bitmap, and starting the parsing process.*  
**Fields**  
``private CodeChar[,] Data``  
The memory representation of the image's color grid.  
``private System.Drawing.Point Entry``  
A Point structure representing the image's entry point.  
**Constructors**  
``public Loader(string Input)``  
Starts the loading and parsing process on Input.  
``public Loader(string Input, int Depth)``  
Starts the loading and parsing process on Input, and passes the recursion depth (see [help/PARSE.md](https://github.com/jay-tux/BMPScript/blob/master/help/PARSE.md)).  
**Methods**  
``protected void Load(string Input, int Depth = 0)``  
Fails if Depth > 100. Uses the ParseBMP class to load the given image into memory, then searches for the entry point, and lastly, invokes the Parser on the program.  

### Jay.BMPScript.Parser
File: ``./Parser.cs``  
Inherits: none  
*The class which does all of the actual parsing work.*  
**Fields**  
``public static Random RNG``  
The RNG used for the ``RNG`` and ``RNGV`` commands.  
``public static OutWriter Writer``  
The output stream, which logs to ``stdout`` and/or the recursive image (see [help/PARSE.md](https://github.com/jay-tux/BMPScript/blob/master/help/PARSE.md)).  
``private Dictionary<CodeChar, Point> Labels``  
The collection which contains all labels set by the program.  
``private Dictionary<int, int> Integers``  
The collection which contains all integers used by the bmp program.  
``private Dictionary<int, char> Characters``  
The collection which contains all characters used by the bmp program.  
``private CodeChar[,] Program``  
The actual bmp program.  
``private int Read``, ``private int Write``, ``private int Depth``  
Helper variables for the PARSE command. See [help/PARSE.md](https://github.com/jay-tux/BMPScript/blob/master/help/PARSE.md).  
**Constructors**  
``public Parser(CodeChar[,] Program, int Depth)``  
Creates a new Parser object with the given program and depth. The depth is only used for the PARSE command.  
**Methods**
``public void Start(Point Entry)``  
Starts the parser on the program at the given entry point.  
``protected CodeChar GetAt(Point Pos)``
A shortcut method for getting the CodeChar at the given position.  
``protected void SysDump()``  
Prints a dump of all labels, integers and characters stored.  
``protected void Overview(Point Entry)``  
Prints a linear overview of the program and its arguments, starting at the given entry point.  
``protected void PreProcess(Point Entry)``  
Preprocesses the program, with starting point Entry. Preprocessing includes the setting of labels, in order to allow for jump-forwards.  
``protected int EvaluateMath(int ID1, int OP, int ID2)``  
Evaluates a mathematical expression, as explained in the MATH section of [the programming guide](https://github.com/jay-tux/BMPScript/blob/master/README.md).  
``protected bool EvaluateCheck(int ID1, int OP, int ID2)``  
Evaluates a check (for an IF or NOT command), as explained in the IF section of [the programming guide](https://github.com/jay-tux/BMPScript/blob/master/README.md).  

### Jay.BMPScript.OutWriter
File: ``./OutWriter.cs``  
Inherits: none  
*A class for writing output to ``stdout`` (and in the future, to the output bitmaps) and for writing debug messages*  
**Fields**  
``private Queue<char> data``  
A Queue for storing the program output, so it may be saved to the output bitmap.  
**Constructors**  
``public OutWriter()``  
Creates a new, empty OutWriter object.  
**Methods**  
``public void Write(string Data)``  
Prints a message to ``stdout`` and enqueues it to the data queue.  
``public static void Debug(string Data)``  
Logs a debug message to ``stderr``. Currently commented out.  
``public void Finish(int OutFile)``![Not implemented yet](https://img.shields.io/static/v1?label=Issue&message=Not%20implemented%20yet&color=critical)  
Writes the output queue to the given output bmp. Not implemented yet.  
``private static void SaveBMP(byte[] buffer, int width, int height, string file)``![Not implemented yet](https://img.shields.io/static/v1?label=Issue&message=Not%20implemented%20yet&color=critical)  
The actual file conversion/writing. Not implemented yet.