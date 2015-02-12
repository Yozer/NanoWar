namespace NanoWar
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml.Linq;

    using SFML.Audio;
    using SFML.Graphics;
    using SFML.System;

    public class ResourceManager
    {
        public delegate object Factory(XElement element);

        private static ResourceManager _instance;

        private readonly Dictionary<string, Asset> _assets = new Dictionary<string, Asset>();

        private readonly Dictionary<string, Factory> _factories = new Dictionary<string, Factory>();

        public ResourceManager()
        {
            var doc = XDocument.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("NanoWar.assets.xml"));
            Load(doc.Element("assets"));
            SfmlFactories.RegisterAllTo(this);
        }

        public static ResourceManager Instance
        {
            get
            {
                return _instance ?? (_instance = new ResourceManager());
            }
        }

        public object this[string id]
        {
            get
            {
                var ret = _assets[id].Wref.Target;
                if (ret != null)
                {
                    return ret;
                }

                var type = _assets[id].Element.Name.ToString();
                try
                {
                    ret = _factories[type](_assets[id].Element);
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }

                _assets[id].Wref.Target = ret;
                return ret;
            }
        }

        public XElement GetAssetNameByPath(string path)
        {
            return _assets[path].Element;
        }

        public void Load(XElement el, string prefix = null)
        {
            if (prefix != null)
            {
                prefix += "/";
            }
            else
            {
                prefix = string.Empty;
            }

            foreach (var element in el.Elements())
            {
                var id = element.Attribute("id");
                if (id == null)
                {
                    continue;
                }

                if (element.Name == "section")
                {
                    Load(element, id.Value);
                }
                else
                {
                    _assets.Add(prefix + id.Value, new Asset(element));
                }
            }
        }

        public void RegisterFactory(string elementName, Factory f)
        {
            _factories.Add(elementName, f);
        }

        private class Asset
        {
            public readonly XElement Element;

            public readonly WeakReference Wref;

            public Asset(XElement el)
            {
                Element = el;
                Wref = new WeakReference(null);
            }
        }
    }

    public static class SfmlFactories
    {
        private static Assembly _assembly = Assembly.LoadFrom(Environment.CurrentDirectory + @"\Assets.dll");

        public static void RegisterAllTo(ResourceManager m)
        {
            m.RegisterFactory("sound", SoundBuffer);
            m.RegisterFactory("music", Music);
            m.RegisterFactory("texture", Texture);
            m.RegisterFactory("image", Image);
            m.RegisterFactory("font", Font);
            m.RegisterFactory("shader", Shader);
        }

        public static string StringAttributeParse(XElement el, string attr, string dfault = null)
        {
            var attribute = el.Attribute(attr);
            if (attribute == null)
            {
                return dfault;
            }

            return attribute.Value;
        }

        public static Stream StreamAttributeParse(XElement el, string prefix = "")
        {
            var res = StringAttributeParse(el, prefix + "res");
            if (res != null)
            {
                return _assembly.GetManifestResourceStream(res.Replace("NanoWar", "Assets"));
            }

            var src = StringAttributeParse(el, prefix + "src");
            if (src != null)
            {
                return new FileStream(src, FileMode.Open, FileAccess.Read);
            }

            return null;
        }

        private static IntRect AreaAttributeParse(XElement el, string attr, IntRect dfault = new IntRect())
        {
            var attribute = el.Attribute(attr);
            if (attribute == null)
            {
                return dfault;
            }

            var ints = attribute.Value.Split(',');
            return new IntRect(int.Parse(ints[0]), int.Parse(ints[1]), int.Parse(ints[2]), int.Parse(ints[3]));
        }

        private static float FloatAttributeParse(XElement el, string attr, float dfault = 0.0f)
        {
            var attribute = el.Attribute(attr);
            if (attribute == null)
            {
                return dfault;
            }

            return float.Parse(attribute.Value);
        }

        public static int IntegerAttributeParse(XElement el, string attr, int dfault = 0)
        {
            var attribute = el.Attribute(attr);
            if (attribute == null)
            {
                return dfault;
            }

            return int.Parse(attribute.Value);
        }

        private static bool BoolAttributeParse(XElement el, string attr, bool dfault = false)
        {
            var attribute = el.Attribute(attr);
            if (attribute == null)
            {
                return dfault;
            }

            return bool.Parse(attribute.Value);
        }

        private static Vector3f VectorAttributeParse(XElement el, string attr, Vector3f dfault = new Vector3f())
        {
            var attribute = el.Attribute(attr);
            if (attribute == null)
            {
                return dfault;
            }

            var ints = attribute.Value.Split(',');
            return new Vector3f(int.Parse(ints[0]), int.Parse(ints[1]), int.Parse(ints[2]));
        }

        // Audio
        public static SoundBuffer SoundBuffer(XElement el)
        {
            return new SoundBuffer(StreamAttributeParse(el));
        }

        public static Music Music(XElement el)
        {
            var m = new Music(StreamAttributeParse(el))
                        {
                            Pitch = FloatAttributeParse(el, "pitch", 1.0f), 
                            Volume = FloatAttributeParse(el, "volume", 100f), 
                            Loop = BoolAttributeParse(el, "loop"), 
                            Position = VectorAttributeParse(el, "position")
                        };
            return m;
        }

        // Graphics
        public static Texture Texture(XElement el)
        {
            var area = AreaAttributeParse(el, "area");
            var tex = new Texture(StreamAttributeParse(el), area)
                          {
                              Smooth = BoolAttributeParse(el, "smooth"), 
                              Repeated = BoolAttributeParse(el, "repeated")
                          };
            return tex;
        }

        public static Image Image(XElement el)
        {
            return new Image(StreamAttributeParse(el));
        }

        public static Font Font(XElement el)
        {
            return new Font(StreamAttributeParse(el));
        }

        public static Shader Shader(XElement el)
        {
            return new Shader(StreamAttributeParse(el, "vertex_"), StreamAttributeParse(el, "fragment_"));
        }
    }
}