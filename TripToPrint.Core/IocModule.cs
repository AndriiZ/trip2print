﻿using Autofac;

using TripToPrint.Core.Logging;
using TripToPrint.Core.ModelFactories;

namespace TripToPrint.Core
{
    public class IocModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Services
            builder.RegisterType<HereAdapter>().As<IHereAdapter>();
            builder.RegisterType<GoogleMyMapAdapter>().As<IGoogleMyMapAdapter>();
            builder.RegisterType<ReportGenerator>().As<IReportGenerator>();
            builder.RegisterType<ReportWriter>().As<IReportWriter>();
            builder.RegisterType<FileService>().As<IFileService>();
            builder.RegisterType<ZipService>().As<IZipService>();
            builder.RegisterType<ResourceNameProvider>().As<IResourceNameProvider>();
            builder.RegisterType<WebClientService>().As<IWebClientService>();

            // Logging
            builder.RegisterType<LogStorage>().As<ILogStorage>().SingleInstance();
            builder.RegisterType<Logger>().As<ILogger>();

            // Class Factories
            builder.RegisterType<ProgressTrackerFactory>().As<IProgressTrackerFactory>();

            // Model Factories
            builder.RegisterType<KmlDocumentFactory>().As<IKmlDocumentFactory>();
            builder.RegisterType<MooiDocumentFactory>().As<IMooiDocumentFactory>();
            builder.RegisterType<MooiGroupFactory>().As<IMooiGroupFactory>();

            base.Load(builder);
        }
    }
}
