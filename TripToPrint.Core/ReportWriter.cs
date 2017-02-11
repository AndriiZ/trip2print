﻿using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IReportWriter
    {
        Task<string> WriteReportAsync(MooiDocument document);
    }

    public class ReportWriter : IReportWriter
    {
        private readonly IResourceNameProvider _resourceName;

        public ReportWriter(IResourceNameProvider resourceName)
        {
            _resourceName = resourceName;
        }

        public async Task<string> WriteReportAsync(MooiDocument document)
        {
            return await Task.Run(() => this.RenderReport(document));
        }

        private string RenderReport(MooiDocument document)
        {
            var sb = new StringBuilder();

            sb.Append(@"<!DOCTYPE html><html xmlns=""http://www.w3.org/1999/xhtml""><head><meta charset=""utf-8"" />");
            sb.Append($"<title>{document.Title}</title>");
            sb.Append(@"<style>
                body { margin: 0; -webkit-print-color-adjust: exact; font-family: Arial }
                h3 { margin: 0; }
                .doc-desc { font-style: italic; color: gray; }
                .ov { padding-top: 5px; overflow: hidden; }
                .ov-notfirst { page-break-before: always; }
                .ov .title { position: absolute; left: 8px; z-index: 2; margin-top: 8px; padding: 1px 6px; display: inline; background: white; border-radius: 8px; border: 1px solid #ccc; }
                .ov img { width: 100%; }
                .pm-cols { overflow: hidden; }
                .pm-col { width: 49.9999%; float: left; }
                .pm { border: 1px solid #ccc; overflow: hidden; margin: 0 1px 2px 0; padding: 1px; page-break-inside: avoid; }
                .pm .title { color: black; font-weight: bold; font-size: 12pt; }
                .pm .header { font-family: 'Calibri Light'; }
                .pm .ix { position: relative; float: left; top: 126px; margin-left: -30px; background: #4189b3; border-radius: 10px; padding: 1px 6px; color: white; font-family: 'Consolas' }
                .pm-desc { font-size: 9.5pt; }
                .pm-img img { max-width: 200px; max-height: 150px; float: left; margin-right: 4px; }
                .icon { width: 30px; position: relative; z-index: 5; float: left; }
                .map { max-height: 150px; position: relative; vertical-align: top; left: -30px; float: left; margin-right: -26px; }
                .coord { color: gray; font-size: 9pt; font-weight: bold; }
                </style>");
            sb.Append(@"</head><body>");

            sb.AppendLine($"<h3>{document.Title}</h3>");
            if (!string.IsNullOrEmpty(document.Description))
            {
                sb.Append($"<p class='doc-desc'>{document.Description}</p>");
            }

            foreach (var folder in document.Sections)
            {
                RenderSection(folder, sb, folder == document.Sections[0]);
            }

            sb.Append(@"</body></html>");

            return sb.ToString();
        }

        private void RenderSection(MooiSection section, StringBuilder sb, bool first)
        {
            sb.AppendLine("<div class='folder'>");
            foreach (var group in section.Groups)
            {
                RenderGroup(group, sb, first);
                first = false;
            }
            sb.AppendLine("</div>");
        }

        private void RenderGroup(MooiGroup group, StringBuilder sb, bool first)
        {
            sb.Append($"<div class='ov {(first ? string.Empty : "ov-notfirst")}'>");
            sb.AppendLine($"<h4 class='title'>{group.Section.Name}</h3>");
            sb.Append($"<img src='{_resourceName.CreateFileNameForOverviewMap(@group)}' />");
            sb.Append("</div>");

            var meaningSizeOfGroup = group.Placemarks.Count + group.Placemarks
                .Select(x => Math.Max(1, CountOfImagesInPlacemark(x)) - 1)
                .Sum();

            sb.Append("<div class='pm-cols'>");
            sb.Append("<div class='pm-col'>");
            var i = 0;
            foreach (var placemark in group.Placemarks)
            {
                if (i >= meaningSizeOfGroup / 2)
                {
                    sb.Append("</div><div class='pm-col'>");
                    i = int.MinValue;
                }

                RenderPlacemark(group, placemark, sb);
                i += Math.Max(1, CountOfImagesInPlacemark(placemark));
            }
            sb.Append("</div>");
            sb.Append("</div>");
        }

        private void RenderPlacemark(MooiGroup group, MooiPlacemark placemark, StringBuilder sb)
        {
            var coordinate = placemark.Coordinate.Latitude.ToString("0.######") + ","
                + placemark.Coordinate.Longitude.ToString("0.######");
            var iconPath = placemark.IconPathIsOnWeb
                ? StringHelper.MakeStringSafeForFileName(placemark.IconPath)
                : $"{placemark.IconPath}";
            sb.Append("<div class='pm'>");
            sb.Append($"<img class='icon' src='{iconPath}' />");
            sb.Append($"<img class='map' src='{_resourceName.CreateFileNameForPlacemarkThumbnail(placemark)}' />");
            sb.Append($"<div class='ix'>{group.Placemarks.IndexOf(placemark) + 1}</div>");
            sb.Append($"<div class='header'><span class='coord'>(<a href='http://maps.google.com/?ll={coordinate}'>{coordinate}</a>)</span> <span class='title'>{placemark.Name}</span></div>");
            if (!string.IsNullOrEmpty(placemark.ImagesContent))
            {
                sb.Append($"<div class='pm-img'>{placemark.ImagesContent}</div>");
            }
            if (!string.IsNullOrEmpty(placemark.Description))
            {
                sb.Append($"<div class='pm-desc'>{placemark.Description}</div>");
            }
            sb.AppendLine("</div>");
        }

        private int CountOfImagesInPlacemark(MooiPlacemark placemark)
        {
            if (string.IsNullOrEmpty(placemark.ImagesContent))
            {
                return 0;
            }

            return Regex.Matches(placemark.ImagesContent, "<img").Count;
        }
    }
}
