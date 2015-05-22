// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using System.Collections.Generic;
using NakedObjects.Surface.Context;
using NakedObjects.Surface.Utility;
using org.nakedobjects.@object;
using org.nakedobjects.@object.control;

namespace NakedObjects.Surface.Nof2.Wrapper {
    public class NakedObjectAssociationWrapper : ScalarPropertyHolder, INakedObjectAssociationSurface {
        private readonly NakedObjectField assoc;
        private readonly Naked target;

        public NakedObjectAssociationWrapper(NakedObjectField assoc, Naked target, INakedObjectsSurface surface) {
            this.assoc = assoc;
            this.target = target;
            Surface = surface;
        }

        public bool IsCollection {
            get { return assoc.isCollection(); }
        }

        public bool IsObject {
            get { return assoc.isObject(); }
        }

        public string Name {
            get { return assoc.getName(); }
        }

        public string Description {
            get { return assoc.getDescription(); }
        }

        public bool IsMandatory {
            get {
                //return assoc.isMandatory(); doesn't seem to be returning much useful - default to true for the moment
                return true;
            }
        }

        public int? MaxLength {
            get { return null; }
        }

        public string Pattern {
            get { return null; }
        }

        protected object MemberOrder {
            get { return 0; }
        }

        protected object IsInline {
            get { return false; }
        }

        #region INakedObjectAssociationSurface Members

        public INakedObjectSpecificationSurface Specification {
            get { return new NakedObjectSpecificationWrapper(assoc.getSpecification(), target, Surface); }
        }

        public INakedObjectSpecificationSurface ElementSpecification {
            get {
             
                return new NakedObjectSpecificationWrapper(org.nakedobjects.@object.NakedObjects.getSpecificationLoader().loadSpecification(typeof(object).FullName), null, Surface);
            
            }
        }
        Choices INakedObjectAssociationSurface.IsChoicesEnabled {
            get { return Choices.NotEnabled; }
        }
        public bool IsAutoCompleteEnabled {
            get { return false; }
        }

        public string Id {
            get {
                //var id = assoc.getName();
                //return id.Substring(0, 1).ToUpper() + id.Substring(1);
                return assoc.getName().Replace(" ", "");
            }
        }

        public bool IsChoicesEnabled {
            get {
                return false;
                //return ((OneToOneFeature) assoc).IsChoicesEnabled;
            }
        }

        public IConsentSurface IsUsable(INakedObjectSurface target) {
            Consent consent = assoc.isAvailable((NakedReference) ((NakedObjectWrapper) target).NakedObject);
            return new ConsentWrapper(consent);
        }

        public INakedObjectSurface GetNakedObject(INakedObjectSurface target) {
            Naked result = assoc.get((NakedObject) ((NakedObjectWrapper) target).NakedObject);
            return result == null ? null : new NakedObjectWrapper(result, Surface);
        }

        public bool IsVisible(INakedObjectSurface nakedObject) {
            return !assoc.isHidden() && assoc.isVisible((NakedReference) ((NakedObjectWrapper) nakedObject).NakedObject).isAllowed();
        }

        public bool IsEager(INakedObjectSurface nakedObject) {
            return false; 
        }

        public INakedObjectSurface[] GetChoices(INakedObjectSurface target, IDictionary<string, object> parameterNameValues) {
            return new INakedObjectSurface[]{};
        }

        public Tuple<string, INakedObjectSpecificationSurface>[] GetChoicesParameters() {
           return new Tuple<string, INakedObjectSpecificationSurface>[]{};
        }

        public Tuple<INakedObjectSurface, string>[] GetChoicesAndTitles(INakedObjectSurface target, IDictionary<string, object> parameterNameValues) {
            return new Tuple<INakedObjectSurface, string>[]{};
        }

        public INakedObjectSurface[] GetCompletions(INakedObjectSurface target, string autoCompleteParm) {
            return new INakedObjectSurface[]{};
        }

        public INakedObjectSurface[] GetChoices(INakedObjectSurface target, IDictionary<string, INakedObjectSurface> parameterNameValues) {
            // return ((OneToOneFeature) assoc).GetChoices(((NakedObject2) target).NakedObject, null).Select(no => new NakedObject2(no)).ToArray();
            return null;
        }

        public int Count(INakedObjectSurface nakedObject) {
            if (IsCollection) {
                var result = (NakedCollection) assoc.get((NakedObject)((NakedObjectWrapper)nakedObject).NakedObject);
                return result.size();
            }
            return 0;
        }

        public string GetMaskedValue(INakedObjectSurface valueNakedObject) {
            throw new NotImplementedException();
        }

        public bool DefaultTypeIsExplicit(INakedObjectSurface nakedObject) {
            throw new NotImplementedException();
        }

        public string GetTitle(INakedObjectSurface nakedObject) {
            return nakedObject.TitleString;
        }

        #endregion

        public override bool Equals(object obj) {
            var nakedObjectAssociationWrapper = obj as NakedObjectAssociationWrapper;
            if (nakedObjectAssociationWrapper != null) {
                return Equals(nakedObjectAssociationWrapper);
            }
            return false;
        }

        public bool Equals(NakedObjectAssociationWrapper other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.assoc, assoc);
        }

        public override int GetHashCode() {
            return (assoc != null ? assoc.GetHashCode() : 0);
        }

        public override object GetScalarProperty(ScalarProperty name) {
            switch (name) {
                case (ScalarProperty.Name):
                    return Name;
                case (ScalarProperty.Description):
                    return Description;
                case (ScalarProperty.IsCollection):
                    return IsCollection;
                case (ScalarProperty.IsObject):
                    return IsObject;
                case (ScalarProperty.IsMandatory):
                    return IsMandatory;
                case (ScalarProperty.MaxLength):
                    return MaxLength;
                case (ScalarProperty.Pattern):
                    return Pattern;
                case (ScalarProperty.MemberOrder):
                    return MemberOrder;
                case (ScalarProperty.IsASet):
                    return false;
                case (ScalarProperty.IsInline):
                    return IsInline;
                case (ScalarProperty.Mask):
                    return "";
                case (ScalarProperty.AutoCompleteMinLength):
                    return 0;
                case (ScalarProperty.IsConcurrency):
                    return false;
                case (ScalarProperty.NumberOfLines):
                    return 0;
                case (ScalarProperty.Width):
                    return 0;
                case (ScalarProperty.TypicalLength):
                    return 0;
                case (ScalarProperty.ExtensionData):
                    return null;
                case (ScalarProperty.TableViewData):
                    return null;
                case (ScalarProperty.RenderEagerly):
                    return false;
                case (ScalarProperty.DoNotCount):
                    return false;
                case (ScalarProperty.IsNullable):
                    return false;
                case (ScalarProperty.IsAjax):
                    return false;
                case (ScalarProperty.IsPassword):
                    return false;
                case (ScalarProperty.Range):
                    return 0;
                case (ScalarProperty.RegEx):
                    return null;
                case (ScalarProperty.IsFindMenuEnabled):
                    return false;
                case (ScalarProperty.PresentationHint):
                    return "";
                case (ScalarProperty.IsEnum):
                    return false;
                case (ScalarProperty.IsFileAttachment):
                    return false;
                default:
                    throw new NotImplementedException(string.Format("{0} doesn't support {1}", GetType(), name));
            }
        }

        public INakedObjectsSurface Surface { get; set; }
    }
}