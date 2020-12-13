# Takonsole
<img alt="screenshot" src="https://raw.githubusercontent.com/TikoTako/TikoTako/main/img/Takonsole.png" height="400"/><br/>
Add a console for debugging to a winform/wpf project.<br/>
It uses the 24 bit RGB ability of the new win 10 console, there is no standard/extended/256 colors support.<br/>
Out, Inf, Warn, Err have different colors (can be changed)<br/>

```c#
public void RawPrintToConsole(string _message, bool _timeStamp, bool newLine, bool resetColors, Color? _char, Color? _back)
```

*Allocate the console and use the RawPrintToConsole (you can make your own method to print stuff)*
```c#
Takonsole con = Takonsole.Alloc("Console window name");
.
.
.
con.RawPrintToConsole("POTATO", true, true, true, f_color, b_color);
```
***or***

*Allocate the console then use the static methods (Faster i use this).*
```c#
_ = Takonsole.Alloc("Console window name");
public static Action<string> Out = Takonsole.Out;
public static Action<string> Inf = Takonsole.Inf;
public static Action<string> Warn = Takonsole.Warn;
public static Action<string> Err = Takonsole.Err;
.
.
.
try
{
    Out("HELLO");
    .
    .
    .
}
catch (Exception ex)
{
    Warn($"HELL {ex.Message}");
```