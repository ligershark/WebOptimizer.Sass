# A Sass compiler for ASP.NET Core

[![Build status](https://ci.appveyor.com/api/projects/status/i4uo3yef1gpyu00y?svg=true)](https://ci.appveyor.com/project/madskristensen/weboptimizer-sass)

This package compiles Sass/Scss into CSS by hooking into the [LigerShark.WebOptimizer](https://github.com/ligershark/WebOptimizer) pipeline.

Here's an example of how to compile `a.scss` and `b.scss` and bundle them into a single .css file called `/all.css`:

```c#
services.AddWebOptimizer(assets =>
{
    assets.AddScss("/all.css", "css/a.scss", "css/b.scss");
});
```