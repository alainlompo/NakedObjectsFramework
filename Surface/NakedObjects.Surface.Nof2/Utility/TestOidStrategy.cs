﻿// Copyright © Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System;
using System.Linq;
using System.Reflection;
using NakedObjects.Surface.Nof2.Implementation;
using NakedObjects.Surface.Nof2.Wrapper;
using NakedObjects.Surface.Utility;
using org.nakedobjects.@object;
using org.nakedobjects.@object.persistence;
using sdm.systems.application.value;
using Action = System.Action;

namespace NakedObjects.Surface.Nof2.Utility {
    // Strategy based on each object having a key called 'Id' 
    public class TestOidStrategy : IOidStrategy {
        private const string sep = "-";

        #region IOidStrategy Members

        public object GetDomainObjectByOid(ILinkObjectId objectId) {
            Type type = ValidateObjectId(objectId);
            string[] keys = GetKeys(objectId.InstanceId, type);

            NakedObjectSpecification spec = org.nakedobjects.@object.NakedObjects.getSpecificationLoader().loadSpecification(type.FullName);

            NakedObject pattern = org.nakedobjects.@object.NakedObjects.getObjectLoader().createTransientInstance(spec);

            PropertyInfo p = pattern.getObject().GetType().GetProperty("Id");
            ((WholeNumber) p.GetValue(pattern.getObject(), null)).setValue(int.Parse(keys.First()));

            var criteria = new TitleCriteria(spec, pattern.titleString(), false);
            TypedNakedCollection results = org.nakedobjects.@object.NakedObjects.getObjectPersistor().findInstances(criteria);

            if (results.size() == 0) {
                throw new ObjectResourceNotFoundNOSException(objectId.ToString());
            }

            return results.elementAt(0).getObject();
        }

        public object GetServiceByServiceName(ILinkObjectId serviceName) {
            Type type = ValidateServiceId(serviceName);

            try {
                return SurfaceUtils.GetServicesInternal().Single(s => s.getSpecification().getFullName() == type.FullName).getObject();
            }
            catch (Exception e) {
                throw new ServiceResourceNotFoundNOSException(serviceName.DomainType, e);
            }
        }

        public ILinkObjectId GetOid(INakedObjectSurface nakedObject) {
            Tuple<string, string> codeAndKey = GetCodeAndKeyAsTuple(nakedObject);
            return new LinkObjectId(codeAndKey.Item1, codeAndKey.Item2);
        }

        public ILinkObjectId GetOid(object domainObject) {
            throw new NotImplementedException();
        }

        public ILinkObjectId GetOid(string servicename) {
            return new LinkObjectId(servicename, "");
        }

        public ILinkObjectId GetOid(string typeName, string instanceId) {
           return new LinkObjectId(typeName, instanceId);
        }

        public string GetObjectId(INakedObjectSurface nakedobject) {
            throw new NotImplementedException();
        }

        public INakedObjectSpecificationSurface GetSpecificationByLinkDomainType(string linkDomainType) {
            Type type = GetType(linkDomainType);
            NakedObjectSpecification spec = org.nakedobjects.@object.NakedObjects.getSpecificationLoader().loadSpecification(type.FullName);
            return new NakedObjectSpecificationWrapper(spec, null, Surface);
        }

        public string GetLinkDomainTypeBySpecification(INakedObjectSpecificationSurface spec) {
            return GetCode(spec);
        }

        public INakedObjectsSurface Surface { set; private get; }

        #endregion

        protected Tuple<string, string> GetCodeAndKeyAsTuple(INakedObjectSurface nakedObject) {
            var code = GetCode(nakedObject.Specification);
            return new Tuple<string, string>(code, GetKeyValues(nakedObject));
        }

        protected static string GetKeyValues(INakedObjectSurface nakedObjectForKey) {
            PropertyInfo keyProperty = nakedObjectForKey.Object.GetType().GetProperty("Id");

            if (keyProperty != null) {
                object key = keyProperty.GetValue(nakedObjectForKey.Object, null);
                var keys = new[] {key.ToString()};
                return GetKeyCodeMapper().CodeFromKey(keys, nakedObjectForKey.Object.GetType());
            }
            return "";
        }

        private static ITypeCodeMapper GetTypeCodeMapper() {
            // if required introduce some form of indirection here 
            return  new DefaultTypeCodeMapper();
        }

        private static IKeyCodeMapper GetKeyCodeMapper() {
            // if required introduce some form of indirection here 
            return new DefaultKeyCodeMapper();
        }

        private static string[] GetKeys(string instanceId, Type type) {
            return GetKeyCodeMapper().KeyFromCode(instanceId, type);
        }

        private static string GetCode(Type type) {
            return GetTypeCodeMapper().CodeFromType(type);
        }

        private static string GetCode(INakedObjectSpecificationSurface spec) {
            return GetCode(SurfaceUtils.GetType(spec.FullName()));
        }

        private static Type GetType(string typeName) {
            return GetTypeCodeMapper().TypeFromCode(typeName);
        }

        private static string[] ExtractTypeAndKeys(string encodedTypeAndKeys) {
            return encodedTypeAndKeys.Split('-');
        }

        protected static Type ValidateServiceId(ILinkObjectId objectId) {
            return ValidateId(objectId, () => { throw new ServiceResourceNotFoundNOSException(objectId.ToString()); });
        }

        protected static Type ValidateObjectId(ILinkObjectId objectId) {
            return ValidateId(objectId, () => { throw new ObjectResourceNotFoundNOSException(objectId.ToString()); });
        }


        private static Type ValidateId(ILinkObjectId objectId, Action onError) {
            if (string.IsNullOrEmpty(objectId.DomainType.Trim())) {
                throw new BadRequestNOSException();
            }

            Type type = GetType(objectId.DomainType);

            if (type == null) {
                onError();
            }

            return type;
        }
    }
}