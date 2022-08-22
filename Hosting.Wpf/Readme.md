# Hosting.Wpf

## What is it used for?
This library adds WPF support to [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting).

## How do I use it?
You configure your host to use WPF and add Window instances as required.
Then you use the provided extension method to run the application.

    class Program {
      [STAThread]
      static int Main(string[] args) {
        var hostBuilder = new HostBuilder();
        hostBuilder.ConfigureWpf(svc =>
        {
            svc.AddSingleton(_ => new Window { Title = "My first Test Window" });
            svc.AddSingleton(_ => new Window { Title = "My second Test Window" });
        });
        return hostBuilder.RunWpf(CancellationToken.None);
      }
    }

## Why no async support?
A WPF application has to be started from a STA thread since many UI components require this.
I have not investigated if it is possible to do so from an async context, but not using it has definitely been an easy solution.
Any hosted services configured will work as usual.