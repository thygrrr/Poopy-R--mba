//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGenerator.ComponentExtensionsGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Entitas {

    public partial class Entity {

        static readonly Dirty dirtyComponent = new Dirty();

        public bool isDirty {
            get { return HasComponent(ComponentIds.Dirty); }
            set {
                if(value != isDirty) {
                    if(value) {
                        AddComponent(ComponentIds.Dirty, dirtyComponent);
                    } else {
                        RemoveComponent(ComponentIds.Dirty);
                    }
                }
            }
        }

        public Entity IsDirty(bool value) {
            isDirty = value;
            return this;
        }
    }

    public partial class Matcher {

        static IMatcher _matcherDirty;

        public static IMatcher Dirty {
            get {
                if(_matcherDirty == null) {
                    var matcher = (Matcher)Matcher.AllOf(ComponentIds.Dirty);
                    matcher.componentNames = ComponentIds.componentNames;
                    _matcherDirty = matcher;
                }

                return _matcherDirty;
            }
        }
    }
}
