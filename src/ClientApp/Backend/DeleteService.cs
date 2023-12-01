﻿using System;
using System.Threading;
using Microsoft.Extensions.Options;
using Modm.Azure;

namespace ClientApp.Backend
{
	public class DeleteService : BackgroundService
    {
        bool deleteStarted;
        string resourceGroupName;

        private readonly DeleteServiceOptions options;
        private readonly AzureDeploymentCleanup cleanup;

        private const string DeleteFileName = "delete.txt";

        const int DefaultWaitDelaySeconds = 30;
        
        public DeleteService(IAzureResourceManager resourceManager, IOptions<DeleteServiceOptions> options)
		{
            this.cleanup = new AzureDeploymentCleanup(resourceManager);
            this.options = options.Value;
		}

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await WaitForDelete(cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                await this.cleanup.DeleteResourcePostDeployment(this.resourceGroupName);
            }
        }

        async Task WaitForDelete(CancellationToken cancellationToken)
        {
            while (!deleteStarted || String.IsNullOrEmpty(this.resourceGroupName))
            {
                await Task.Delay(DefaultWaitDelaySeconds * 1000, cancellationToken);

                string stateFileContent = ReadStateFile();
                if (!String.IsNullOrEmpty(stateFileContent))
                {
                    this.deleteStarted = true;
                    this.resourceGroupName = stateFileContent;
                }
            }
        }

        public void Start(string resourceGroupName)
        {
            this.resourceGroupName = resourceGroupName;
            this.deleteStarted = true;
            WriteStateFile(resourceGroupName);
        }

        private string ReadStateFile()
        {
            string filePath = Path.Combine(this.options.DataDirectory, DeleteFileName);

            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }

            return null;
        }

        private void WriteStateFile(string resourceGroupName)
        {
            string filePath = Path.Combine(this.options.DataDirectory, DeleteFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, resourceGroupName);
        }
    }
}
