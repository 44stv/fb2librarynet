﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FB2Library.Elements.Poem;
using FB2Library.Elements.Table;

namespace FB2Library.Elements
{
    public class SectionItem : IFb2TextItem
    {
        internal const string Fb2TextSectionElementName = "section";

        private readonly List<IFb2TextItem> content = new List<IFb2TextItem>();
        private readonly List<EpigraphItem> epigraphs = new List<EpigraphItem>();

        // "pointers" top all section's subsections
        private readonly List<SectionItem> subSections = new List<SectionItem>();

        // "pointers" to all section's images
        private readonly List<ImageItem> images = new List<ImageItem>();

        private ImageItem sectionImage = null;
        




        public List<EpigraphItem> Epigraphs { get { return epigraphs; } }

        public TitleItem Title { get; set; }

        public List<IFb2TextItem> Content { get { return content; } }

        public ImageItem SectionImage { get { return sectionImage; } }

        public AnnotationItem Annotation { get; set; }

        public string ID { get; set; }

        public List<SectionItem> SubSections { get { return subSections; } }

        public List<ImageItem> Images { get { return images; } }

        public string Lang { get; set;}


        internal void Load(XElement xSection)
        {
            ClearAll();
            if (xSection == null)
            {
                throw new ArgumentNullException("xSection");
            }

            if (xSection.Name.LocalName != Fb2TextSectionElementName)
            {
                throw new ArgumentException("Element of wrong type passed", "xSection");
            }

            XElement xTitle = xSection.Element(xSection.Name.Namespace + TitleItem.Fb2TitleElementName);
            if (xTitle != null)
            {
                Title = new TitleItem();
                try
                {
                    Title.Load(xTitle);
                }
                catch (Exception ex)
                {
                    Debug.Fail(string.Format("Failed to load section title : {0}.", ex.Message));
                }
            }
            
            IEnumerable<XElement> xEpigraphs =
                xSection.Elements(xSection.Name.Namespace + EpigraphItem.Fb2EpigraphElementName);
            foreach (var xEpigraph in xEpigraphs)
            {
                EpigraphItem epigraph = new EpigraphItem();
                try
                {
                    epigraph.Load(xEpigraph);
                    epigraphs.Add(epigraph);
                }
                catch (Exception ex)
                {
                    Debug.Fail(string.Format("Failed to load section epigraph : {0}.", ex.Message));
                }
            }

            XElement xAnnotation = xSection.Element(xSection.Name.Namespace + AnnotationItem.Fb2AnnotationItemName);
            if (xAnnotation != null)
            {
                Annotation  = new AnnotationItem();
                try
                {
                    Annotation.Load(xAnnotation);
                }
                catch (Exception ex)
                {
                    Debug.Fail(string.Format("Failed to load section annotation : {0}.", ex.Message));
                }
            }

            IEnumerable<XElement> xElements = xSection.Elements();
            foreach (var xElement in xElements)
            {
                switch (xElement.Name.LocalName)
                {
                    case ParagraphItem.Fb2ParagraphElementName:
                        ParagraphItem paragraph = new ParagraphItem();
                        try
                        {
                            paragraph.Load(xElement);
                            content.Add(paragraph);
                        }
                        catch (Exception ex)
                        {
                            Debug.Fail(string.Format("Failed to load section paragraph : {0}.", ex.Message));
                        }
                        break;
                    case PoemItem.Fb2PoemElementName:
                        PoemItem poem = new PoemItem();
                        try
                        {
                            poem.Load(xElement);
                            content.Add(poem);
                        }
                        catch (Exception ex)
                        {
                            Debug.Fail(string.Format("Failed to load section poem : {0}.", ex.Message));
                        }
                        break;
                    case ImageItem.Fb2ImageElementName:
                        ImageItem image = new ImageItem();
                        try
                        {
                            image.Load(xElement);
                            AddImage(image);
                        }
                        catch (Exception ex)
                        {
                            Debug.Fail(string.Format("Failed to load section image : {0}.", ex.Message));
                        }
                        break;
                    case SubTitleItem.Fb2SubtitleElementName:
                        SubTitleItem subtitle = new SubTitleItem();
                        try
                        {
                            subtitle.Load(xElement);
                            content.Add(subtitle);
                        }
                        catch (Exception ex)
                        {
                            Debug.Fail(string.Format("Failed to load section subtitle : {0}.", ex.Message));
                        }
                        break;
                    case CiteItem.Fb2CiteElementName:
                        CiteItem cite = new CiteItem();
                        try
                        {
                            cite.Load(xElement);
                            content.Add(cite);
                        }
                        catch (Exception ex)
                        {
                            Debug.Fail(string.Format("Failed to load section citation : {0}.", ex.Message));
                        }
                        break;
                    case EmptyLineItem.Fb2EmptyLineElementName:
                        EmptyLineItem eline = new EmptyLineItem();
                        content.Add(eline);
                        break;
                    case TableItem.Fb2TableElementName:
                        TableItem table = new TableItem();
                        try
                        {
                            table.Load(xElement);
                            content.Add(table);
                        }
                        catch (Exception ex)
                        {
                            Debug.Fail(string.Format("Failed to load section emptly line : {0}.", ex.Message));
                        }
                        break;
                    case Fb2TextSectionElementName: // internal <section> read recurive
                        SectionItem section = new SectionItem();
                        try
                        {
                            section.Load(xElement);
                            AddSection(section);
                        }
                        catch (Exception ex)
                        {
                            Debug.Fail(string.Format("Failed to load sub-section : {0}.", ex.Message));
                        }
                        break;
                    case AnnotationItem.Fb2AnnotationItemName: // already processed
                        break; 
                    case TitleItem.Fb2TitleElementName: // already processed
                        break;
                    case EpigraphItem.Fb2EpigraphElementName: // already processed
                        break;
                    default:
                        Debug.Fail(string.Format("AnnotationItem:Load - invalid element <{0}> encountered in title ."), xElement.Name.LocalName);
                        break;
                }
            }

            ID = null;
            XAttribute xID = xSection.Attribute("id");
            if ((xID != null) && (xID.Value != null))
            {
                ID = xID.Value;
            }

            Lang = null;
            XAttribute xLang = xSection.Attribute(XNamespace.Xml + "lang");
            if ((xLang != null)&&(xLang.Value != null))
            {
                Lang = xLang.Value;
            }
        }

        private void ClearAll()
        {
            content.Clear();
            subSections.Clear();
            images.Clear();
            sectionImage = null;
            epigraphs.Clear();
            ID = null;
            Annotation = null;
            Title = null;
        }

        private void AddImage(ImageItem image)
        {
            content.Add(image);
            images.Add(image);
            DetectSectionImage(image);
        }

        private void DetectSectionImage(ImageItem image)
        {
            foreach (var item in content)
            {
                if (item.GetType() == typeof(ImageItem))
                {
                    if (item != image)
                    {
                        Debug.Fail("SectionItem.DetectSectionImage - something wrong, the first image in image list is not first image in content list");
                        return;
                    }
                    sectionImage = image;
                    return;
                }
                if (item.GetType() == typeof(TitleItem))
                {
                    // title allowed
                    // but shouldn't be here
                    Debug.Fail("SectionItem.DetectSectionImage - something wrong, the title should be separate from content list");
                    continue;
                }
                if (item.GetType() == typeof(EpigraphItem))
                {
                    // epigraphs allowed
                    // but shouldn't be here
                    Debug.Fail("SectionItem.DetectSectionImage - something wrong, the epigraphs should be separate from content list");
                    continue;
                }
                // wrong type means we passed possible section image location
                return;
            }
        }

        private void AddSection(SectionItem section)
        {
            content.Add(section);
            subSections.Add(section);
        }
    }
}