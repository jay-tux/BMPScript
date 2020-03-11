# Jay's BMPScript
*An esoteric programming language, where both input and output are BMP images.*
## Programming Guide
### Introduction
Short and sweet: each pixel in your bitmap is parsed as either a command, or the arguments for the last command.
#### Variables
BMPScript can (theoretically) accept up to 512 (different) variables, of which up to 256 can be integers and up to 256 can be characters. Due to practical reasons, only 256 variables can be defined and used.
BMPScript uses two datatypes (they are represented by a range of integers):  
 * integers, spanning ``128-255``,  
 * characters, spanning ``0-127``  
There is (currently) no way to define custom datatypes.
#### Operators
BMPScript has only comparison operators and mathematical operators:  
 * Comparison operators:  
   * ``<``, spanning ``0-63``,  
   * ``==``, spanning ``64-127``,  
   * ``!=``, spanning ``128-191``, and  
   * ``>``, spanning ``192-255``
 * Mathematical operators (memory trick: smaller operator value usually yields a smaller result):  
   * ``/``, spanning ``0-63``,  
   * ``-``, spanning ``64-127``,  
   * ``+``, spanning ``128-191``, and  
   * ``*``, spanning ``192-255``
#### Normal Order of Execution
Because BMPScript is an esoteric programming language, we won't parse your image top-to-bottom, left-to-right, but in a snake-like way, starting at the bottom-right pixel.  
![Start bottom-right, then up](https://github.com/jay-tux/BMPScript/blob/master/help/orderofexec.png  "Order of execution")
#### Commands
BMPScript has 16 possible commands, each of which has a range of colors describing the exact same command. This allows for the whole RGB ``(0-255, 0-255, 0-255)`` to be parsable as a command. Complementary commands (mostly) use complementary colors as well.   
***
**ENTRY**  
The ENTRY command sets the starting point of the program. If no entry point is set, the most bottom-right pixel of the image is used.  
*Parameters:* n/a  
*Color Range:*
***
**WRITE_V**  
Prints a single variable to ``stdout``, without a trailing newline. Depending on the type of the variable, either a character or an integer is printed.  
*Parameters:* the variable to print, in the RED slot of the next pixel.  
*Color Range:*
***
**WRITE_C**  
Prints up to three characters to ``stdout``, also without trailing newline.  
*Parameters:* up to three ASCII character values, respectively in the RED, GREEN and BLACK slots of the next pixel.  
*Color Range:*
***
**WRITE_LN**  
Prints a newline ``\n`` or ``\r\n`` to ``stdout``.  
*Parameters:* n/a  
*Color Range:* 
***
**LABEL**  
Creates a new label at the next pixel. This label can be used for jumping.  
*Parameters:* the label name, spanning the whole RGB range.  
*Color Range:*
***
**IF**  
Is a conditional jump, which uses three pixels (one for the if, one for the comparison, one as label data). If one of the variables doesn't exist, it is parsed as a constant. If one of the variables is a character, it is converted to an integer before comparing.  
*Parameters:* variable 1 (in slot RED), comparison operator (in slot GREEN), variable 2 (in slot BLUE) and a label name (in the second argument pixel).  
*Color Range:*
***
**MATH**  
Either declares or changes an integer to the result of a certain mathematical operation. Nonexistent variables are parsed as constants, and characters are converted to their ASCII value before performing the operation.  
*Parameters:* variable 1 (in slot RED), operator (in slot GREEN), variable 2 (in slot BLUE) and the out/result variable (in slot RED of the second argument pixel).  
*Color Range:*
***
**RNG**  
Sets an (integer) variable to a random value between two bounds. The bounds are expected to be constants, but can be in any order. The lower bound is the minimum of both bounds, and the upper bound is the other.  
*Parameters:* out/result variable (in slot RED), (lower) bound (in slot GREEN), and (upper) bound (in slot BLUE).  
*Color Range:*
***
**RNGV**  
A more generic version of the **RNG** command, in which the bounds can be either variables (integers or characters) or constants.  
*Parameters:* out/result variable (in slot RED), (lower) bound (in slot GREEN) and (upper) bound (in slot BLUE).  
*Color Range:*
***
**PARSE**  
Starts a subprocess which runs another BMPScript instance. See [PARSE](https://github.com/jay-tux/BMPScript/blob/master/help/PARSE.md) .  
*Parameters:*  n/a.  
*Color Range:*
***
**NOT**  
Inverted version of the **IF** command.  
*Parameters:* variable 1 (in slot RED), comparison operator (in slot GREEN), variable 2 (in slot BLUE) and a label name (in the second argument pixel).  
*Color Range:*
***
**JUMP**  
Jumps to a defined label.  
*Parameters:* a label name (spanning the whole RGB range).  
*Color Range:*
***
**VAR_CP**  
Copies (and converts) a variable to a new name. If the source variable doesn't exist, it is parsed as a constant value (either integer or ASCII character).  
*Parameters:* the new variable type (in the RED slot), the destination variable (in the GREEN slot), and the source variable (in the BLUE slot)  
*Color Range:*
***
**VAR**  
Defines a new variable, or changes an existing variable.  
*Parameters:* the variable type (in the RED slot), the new variable name (in the GREEN slot), constant value (in the BLUE slot).  
*Color Range:*
***
**READ**  
Reads a value from ``stdin`` to a variable. Keeps re-asking the user for input until the input is correctly formatted (either an integer, or a string with ``length >= 1``, of which ``input[0]`` is stored)  
*Parameters:* the variable type (in the RED slot), and the variable name (in the GREEN slot).  
*Color Range:*
***
**EXIT**  
Terminates execution.  
*Parameters:* n/a  
*Color Range:*
## Compiling/Running
When compiling, make sure to reference ``System.Drawing`` and set the main class to ``Jay.BMPScript.Program``. Alternatively, there's a compiling/running script in this repo as well: ``./compileRun.sh``. The compiling/running script uses Mono for both compiling and running, so make sure you have that installed (the ``mono`` and ``mcs`` (Mono C# Compiler) commands are required). 
### ./compileRun.sh
The script accepts three arguments:  
 * ``-h`` shows a help message for the compile/run script.  
 * ``-c`` compiles all ``*.cs`` files into one binary (``bin/bmpscript.exe``).  
 * ``-r`` runs the ``bin/bmpscript.exe`` with Mono.
If you pass both the ``-c`` and ``-r`` options and the compilation fails, mono isn't invoke on ``bin/bmpscript.exe``.
## Modifying
All data about modifying and/or extending BMPScript are in the 