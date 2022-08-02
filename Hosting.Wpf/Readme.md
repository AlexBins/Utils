# Hosting.Wpf

## What is it used for?
This library adds WPF support to [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting).

## How do I use it?
You configure your host to contain pages, which are displayed in the WPF main window inside a TabControl.
Then you use the provided extension method to run the application.

    class Program {
      [STAThread]
      static int Main(string[] args) {
      	var hostBuilder = new HostBuilder();
        hostBuilder.ConfigureWpf(svc => svc.AddSingleton(
          _ => new Page {
            Title = "Test Title",
            Background = Brushes.Blue,
          }));
        return hostBuilder.RunWpf(CancellationToken.None);
      }
    }
## Why no async support?
A WPF application has to be started from a STA thread since many UI components require this.
I have not investigated if it is possible to do so from an async context, but not using it has definitely been an easy solution.
Any hosted services configured will work as usual.