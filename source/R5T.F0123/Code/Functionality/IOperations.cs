using System;
using System.Collections.Generic;
using System.Linq;

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
                    var kindMarkedFullPropertyName = instance.KindMarkedFullMemberName.AsKindMarkedFullPropertyName();

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
