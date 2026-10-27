// --------------------------------------------------------------------------------------------
// Copyright (c) 2019 The CefNet Authors. All rights reserved.
// Licensed under the MIT license.
// See the licence file in the project root for full license information.
// --------------------------------------------------------------------------------------------

using CppAst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CefGen
{
	sealed class CefApiClass : CppTypeDeclaration, ICppDeclarationContainer, ICppMember, ICppElement, ICppContainer
	{
		public CefApiClass(string name)
			: base(CppTypeKind.StructOrClass)
		{
			this.Name = name;
			this.Fields = new CppContainerList<CppField>(this);
			this.Functions = new CppContainerList<CppFunction>(this);
			this.Enums = new CppContainerList<CppEnum>(this);
			this.Classes = new CppContainerList<CppClass>(this);
			this.Typedefs = new CppContainerList<CppTypedef>(this);
			this.Properties = new CppContainerList<CppProperty>(this);
		}

		public int ApiVersion { get; set; }

		/// <summary>
		/// Gets the exported CEF functions. CppAst elements belong to a single container,
		/// so they are listed here rather than in <see cref="Functions"/>.
		/// </summary>
		public List<CppFunction> ApiFunctions { get; } = new List<CppFunction>();

		public string ApiHashWindows { get; set; }

		public string ApiHashMacOS { get; set; }

		public string ApiHashLinux { get; set; }

		public override int SizeOf { get; set; }

		public CppContainerList<CppField> Fields { get; set; }

		public CppContainerList<CppFunction> Functions { get; set; }

		public CppContainerList<CppEnum> Enums { get; set; }

		public CppContainerList<CppClass> Classes { get; set; }

		public CppContainerList<CppTypedef> Typedefs { get; set; }

		public CppContainerList<CppProperty> Properties { get; set; }

		public List<CppAttribute> Attributes { get; } = new List<CppAttribute>();

		[Obsolete]
		public List<CppAttribute> TokenAttributes { get; } = new List<CppAttribute>();

		public MetaAttributeMap MetaAttributes { get; } = new MetaAttributeMap();

		public string Name { get; set; }

		public override CppType GetCanonicalType()
		{
			return this;
		}

		public override IEnumerable<ICppDeclaration> Children()
		{
			foreach (CppEnum @enum in Enums)
				yield return @enum;

			foreach (CppClass @class in Classes)
				yield return @class;

			foreach (CppTypedef typedef in Typedefs)
				yield return typedef;

			foreach (CppField field in Fields)
				yield return field;

			foreach (CppFunction function in ApiFunctions.OrderBy(f => f.Name))
				yield return function;
		}
	}
}
