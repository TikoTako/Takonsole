# Takonsole
<img alt="screenshot" src="https://raw.githubusercontent.com/TikoTako/TikoTako/main/img/ss%201.png" height="300"/><br/>
Add a console for debugging to a winform/wpf project.<br/>
It uses the 24 bit RGB ability of the new win 10 console, there is no standard/extended/256 colors support.<br/>
Out, Inf, Warn, Err have different colors (can be changed)<br/>

There is a test program here >> [TakonsoleTest on GitHub](https://github.com/TikoTako/TakonsoleTest) <<

**I apologise for my bad english, i'm italian.**

**Version 1.0**

First release.


**Version 1.1**

- Lots of code has been rewritten.
- The class is full static now and is split in 3 files.
- *Takonsole* contain the main code
- *Takonsole.Imports* and *Takonsole.Structs* contain the DLLImport and relative structs.
- *Takonsole.Message* contain the code for creating the strings.

Add:
- *TakonsoleExceptions* contain the class for the exceptions.


**Code:**
```c#
public static bool Alloc(string ConsoleWindowTitle)
public static bool Alloc(string consoleWindowTitle, Encoding encoding)
```
*Call AllocConsole(), setup the window title, set the console mode, set the encoding, set the output stream, get the font used by the console*
<br/><br/>
```c#
public static void DeAlloc()
```
*Free the console.*
<br/><br/>
```c#
public static void SetFont(string fontName, short? fontSize)
```
*Set the console font.<br/>
The font must follow the microsoft guideline (truetype, fixed size, not Italic, withlut negative A/C spaces).<br/>
Some examples: "Consolas" "Lucida Console"*
<br/><br/>
```c#
public static void Out(string message)
public static void Inf(string message)
public static void Warn(string message)
public static void Err(string message)
```
*Write '[timestamp] message' in different colors, without a normal style (not bold/underlined)<br/>
The timestamp is set by TimeStampFormat<br/>
The colors are set in the properties:<br/>
NormalColor<br/>
TimestampColor<br/>
InformationColor<br/>
WarningColor<br/>
ErrorColor<br/>
BackgroundColor*
<br/><br/>

```c#
public static void Write(string str, Style? style, Color? fontColor, Color? backgroundColor)
public static void WriteLine(string message, Style? style, Color? fontColor, Color? backgroundColor)
```
*Write 'message' without timestamp, is possible to set one or more parameters*
<br/><br/>

```c#
public static void Write(string str)
public static void WriteLine(string str)
```
*If no style/colors are needed use this (it check if there is the console allocated before calling Console.Write)*
<br/><br/>

```c#
public static string GenerateRawFromString(string str, Style? style, Color? fontColor, Color? backgroundColor)
```
*Same as the write but return a string instead of writing so you can combine them and print later.*
<br/><br/>

....
There is more but is kinda hard to explain with my broken english maybe is better to read directly the source of [TakonsoleTest on GitHub](https://github.com/TikoTako/TakonsoleTest)