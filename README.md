# VLispProfiler

VLispProfiler is a tool for AutoCAD to measure the
performance of applications built with Visual LISP.

Use this tool to see where your application is spending
it's time and guide where you should optimize.

**Requires AutoCAD 2011+ Full (not LT) to use.**

## Features
- Easy to use installer which automatically sets up AutoCAD.
- One simple to use AutoCAD command: `prof`.
- Profile your LISP files and optionally specify an entry function.
- Profile output includes: elapsed time, self-elapsed time, run count, and file position.
- Interactive setup program for advanced configurations.
- Installs to current user, no admin required.

## Get Started

### Installing
Download and run the latest installer. See [Releases](https://github.com/talanc/vlisp-profiler/releases).

![Install](docs/vlisp-profiler-install.gif)

### Profiling
1. Open AutoCAD
2. Type `prof`
3. Select a LISP file to profile.
4. Type in an entry function, leave empty and press `Enter` if the file doesn't have one.
5. Observe profiling results.

#### Profiling the whole file
Use this method when there's no entry function in your file.
For example: when `(load "file")` loads and runs your program.

![Profile file](docs/vlisp-profiler-run-file.gif)

#### Profiling from entry function
Use this method when there's an entry function in your file.
For example: when `(load "file")` loads your program and you follow it up with `(entry-func)` to run your program.
![Profile function](docs/vlisp-profiler-run-func.gif)
