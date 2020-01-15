# AddWarmUpMethodToCctor

This program let programmers to safely use Type.TypeInitializer.Invoke(null, null).

# Background

See [this gist](https://gist.github.com/tetsuzin/05295363e13c5c50197d90dd9dfaa0f2).

It throws an error at line 11th.
The error happens when you initialize struct type field in your static constructor and you invoke cctor method.

Simple and best solution code is below.

```
namespace ConsoleApp1
{
    public class Hoge
    {
        public static int Int;
        public static Fuga Fuga;

        static Hoge()
        {
            WarmUp();
            Int = 10;
            Fuga = new Fuga();
        }

        static void WarmUp() {}
    }

    public struct Fuga { }

    class Program
    {
        static void Main(string[] args)
        {
            typeof(Hoge).TypeInitializer.Invoke(null, null);
        }
    }
}
```

You can avoid the error by just calling static empty method before accessing the static struct-kind field.

# How to use

Provide that the current working directory us the root directory of this repository and terminal is PowerShell.

```
dotnet restore
dotnet run add "TargetDll.exe"
```

You should substitute `TargetDll.exe` with your appropriate managed dll or executable file.

# LICENSE

MIT LICENSE

# Special Thanks

I made this repository for [@tetsuzin](https://github.com/tetsuzin).