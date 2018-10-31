# Parallel

This is a simple parallel task runner, intended to be used from the command line. The outputs of each command is by default redirected to the console from where `parallel.exe` is invoked. This can be changed.

Commands can be specified by using the `-c` option as many times as needed. Each `-c` option must be followed by one command enclosed in double quotes.

Alternatively, a file may be used to hold a bunch of commands, each on a new line. The file can be passed in using the `-f` option.
Usage is as shown beow:

```cmd
Usage: parallel.exe -c "<COMMAND>" [-f COMMAND_INPUT_FILE] [-s]
  -c    Specifies a command as COMMAND, that can be executed from the command line.
  -f    Specifies a file (COMMAND_INPUT_FILE) containing a list of commands, each command being on a separate line.
  -s    Show each process. Using this switch would open each task with a separate OS shell.
```

Both `-c` and `-f` may be used together (and multiple times). The commands will be triggered in the order that they are declared on the command line.
