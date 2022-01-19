using System.Collections.Generic;

namespace Bake.Tests.Helpers
{
    public class DockerArguments
    {
        public static DockerArguments With(string image) => new(image);

        private readonly Dictionary<int, int> _ports = new();
        private readonly Dictionary<string, string> _environmentVariables = new();

        public string Image { get; }
        public IReadOnlyDictionary<int, int> Ports => _ports;
        public IReadOnlyDictionary<string, string> EnvironmentVariables => _environmentVariables;

        private DockerArguments(
            string image)
        {
            Image = image;
        }

        public DockerArguments WithPort(int containerPort)
        {
            return WithPort(containerPort, SocketHelper.FreeTcpPort());
        }

        public DockerArguments WithPort(int containerPort, int hostPort)
        {
            _ports[containerPort] = hostPort;
            return this;
        }

        public DockerArguments WithEnvironmentVariable(string name, string value)
        {
            _environmentVariables[name] = value;
            return this;
        }
    }
}
