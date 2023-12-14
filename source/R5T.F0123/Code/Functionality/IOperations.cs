using System;
using System.Collections.Generic;
using System.Linq;

using R5T.L0065.T000;
using R5T.T0132;
using R5T.T0161;
using R5T.T0161.Extensions;
using R5T.T0170;
using R5T.T0172;


namespace R5T.F0123
{
    [FunctionalityMarker]
    public partial interface IOperations : IFunctionalityMarker
    {
        public IEnumerable<Signature> Enumerate_Signatures(IEnumerable<InstanceDescriptor> instances)
        {
            var output = instances
                .Select(instance =>
                {
                    var signature = Instances.SignatureStringOperator.Get_Signature(instance.SignatureString);
                    return signature;
                });

            return output;
        }

        /// <summary>
        /// Gets signature instances from the signature string values of the input instances.
        /// The instance descriptor is included in output tuples to allow associating the signature with the extra information the instance descriptor contains.
        /// (There is no guarantee of uniqueness across the data values of the instance descriptor, so a dictionary mapping is out.)
        /// </summary>
        public IEnumerable<(InstanceDescriptor Instance, Signature Signature)> Enumerate_SignaturesAndInstances(IEnumerable<InstanceDescriptor> instances)
        {
            var output = instances
                .Select(instance =>
                {
                    var signature = Instances.SignatureStringOperator.Get_Signature(instance.SignatureString);

                    return (instance, signature);
                });

            return output;
        }

        public IEnumerable<(InstanceDescriptor Instance, TSignature Signature)> Enumerate_SignaturesAndInstancesAs<TSignature>(IEnumerable<InstanceDescriptor> instances)
            where TSignature : class
        {
            var output = instances
                .Select(instance =>
                {
                    var signature = Instances.SignatureStringOperator.Get_Signature(instance.SignatureString);

                    var signatureAsType = signature as TSignature;

                    return (instance, signatureAsType);
                });

            return output;
        }

        public IEnumerable<TSignature> Enumerate_SignaturesCastTo<TSignature>(IEnumerable<InstanceDescriptor> instances)
            where TSignature : Signature
        {
            var output = this.Enumerate_Signatures(instances)
                .Cast<TSignature>()
                ;

            return output;
        }

        public Signature[] Get_Signatures(IEnumerable<InstanceDescriptor> instances)
        {
            var output = this.Enumerate_Signatures(instances)
                .Now();

            return output;
        }

        public (InstanceDescriptor, Signature)[] Get_SignaturesAndInstances(IEnumerable<InstanceDescriptor> instances)
        {
            var output = this.Enumerate_SignaturesAndInstances(instances)
                .Now();

            return output;
        }

        public TSignature[] Get_SignaturesCastedTo<TSignature>(IEnumerable<InstanceDescriptor> instances)
            where TSignature : Signature
        {
            var output = this.Get_SignaturesCastedTo<TSignature>(instances)
                .Now();

            return output;
        }

        public IEnumerable<IGrouping<TypeSignature, THasDeclaringType>> Group_ByDeclaringType<THasDeclaringType>(IEnumerable<THasDeclaringType> propertySignatures)
            where THasDeclaringType : IHasDeclaringType
        {
            var output = propertySignatures.GroupBy(
                x => x.DeclaringType,
                IdentityBasedTypeSignatureEqualityComparer.Instance);

            return output;
        }

        public IEnumerable<IGrouping<TypeSignature, T>> Group_ByDeclaringType<T>(
            IEnumerable<T> items,
            Func<T, TypeSignature> declaringTypeSelector)
        {
            var output = items.GroupBy(
                declaringTypeSelector,
                IdentityBasedTypeSignatureEqualityComparer.Instance);

            return output;
        }

        public IEnumerable<IGrouping<TypeSignature, (InstanceDescriptor Instance, T Signature)>> Group_ByDeclaringType<T>(
            IEnumerable<(InstanceDescriptor Instance, T Signature)> pairs)
            where T : IHasDeclaringType
        {
            var output = this.Group_ByDeclaringType(
               pairs,
               x => x.Signature.DeclaringType);

            return output;
        }

        /// <summary>
        /// Property name-output type tuples are grouped by {Project File Path}:{Property Declaring Type Namespaced Type Name}.
        /// </summary>
        public ((
            string Key,
            ISimpleTypeName SimpleTypeName,
            INamespacedTypeName NamespacedTypeName,
            IProjectFilePath ProjectFilePath) ContainingTypeInformation,
            (InstanceDescriptor Instance,
            ISimplePropertyName SimplePropertyName,
            IOutputTypeName OutputTypeName)[] InstancesInformation)[]
        Group_PropertiesAndDeclaringType(IEnumerable<InstanceDescriptor> instances)
        {
            var groups = instances
                .Select(instance =>
                {
                    var kindMarkedFullPropertyName = instance.IdentityString.Value.ToKindMarkedFullMemberName().AsKindMarkedFullPropertyName();

                    var (simpleTypeName, namespacedTypeName, namespacedTypedPropertyName, fullPropertyName)
                        = Instances.MemberNameOperator.Get_SimpleTypeName(kindMarkedFullPropertyName);

                    var simplePropertyName = Instances.MemberNameOperator.Get_SimplePropertyName(
                        namespacedTypedPropertyName);

                    var outputType = Instances.MemberNameOperator.Get_OutputTypeName(fullPropertyName);

                    return (instance, simpleTypeName, namespacedTypeName, simplePropertyName, outputType);
                })
                .GroupBy(x =>
                {
                    var key = $"{x.instance.ProjectFilePath}:{x.namespacedTypeName}";
                    return key;
                })
                .Select(group =>
                {
                    // There will always be a first, since group must have a member to become a group in the group-by operator.
                    var (instance, simpleTypeName, namespacedTypeName, _, _) = group.First();

                    return (
                        (
                            group.Key,
                            simpleTypeName,
                            namespacedTypeName,
                            instance.ProjectFilePath),
                        group.Select(x =>
                        {
                            return (
                                x.instance,
                                x.simplePropertyName,
                                x.outputType);
                        })
                        .ToArray());
                })
                .ToArray()
                ;

            return groups;
        }
    }
}
