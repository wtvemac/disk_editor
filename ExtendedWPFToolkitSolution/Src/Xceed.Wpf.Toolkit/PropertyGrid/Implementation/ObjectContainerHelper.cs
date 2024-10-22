﻿/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal class ObjectContainerHelper : ObjectContainerHelperBase
  {
    private object _selectedObject;

    public ObjectContainerHelper( IPropertyContainer propertyContainer, object selectedObject )
      : base( propertyContainer )
    {
      _selectedObject = selectedObject;
    }

    private object SelectedObject
    {
      get
      {
        return _selectedObject;
      }
    }

    protected override string GetDefaultPropertyName()
    {
      object selectedObject = SelectedObject;
      return ( selectedObject != null ) ? ObjectContainerHelperBase.GetDefaultPropertyName( SelectedObject ) : ( string )null;
    }

    protected override IEnumerable<PropertyItem> GenerateSubPropertiesCore()
    {
      var propertyItems = new List<PropertyItem>();

      if( SelectedObject != null )
      {
        try
        {
          List<PropertyDescriptor> descriptors = ObjectContainerHelperBase.GetPropertyDescriptors( SelectedObject );
          foreach( var descriptor in descriptors )
          {
            var propertyDef = this.GetPropertyDefinition( descriptor );
            bool isBrowsable = descriptor.IsBrowsable && this.PropertyContainer.AutoGenerateProperties;
            if( propertyDef != null )
            {
              isBrowsable = propertyDef.IsBrowsable.GetValueOrDefault( isBrowsable );
            }
            if( isBrowsable )
            {
              propertyItems.Add( this.CreatePropertyItem( descriptor, propertyDef ) );
            }
          }
        }
        catch( Exception e )
        {
          //TODO: handle this some how
          Debug.WriteLine( "Property creation failed" );
          Debug.WriteLine( e.StackTrace );
        }
      }

      return propertyItems;
    }


    private PropertyItem CreatePropertyItem( PropertyDescriptor property, PropertyDefinition propertyDef )
    {
      DescriptorPropertyDefinition definition = new DescriptorPropertyDefinition( property, SelectedObject, this.PropertyContainer.IsCategorized );
      definition.InitProperties();

      this.InitializeDescriptorDefinition( definition, propertyDef );

      PropertyItem propertyItem = new PropertyItem( definition );
      Debug.Assert( SelectedObject != null );
      propertyItem.Instance = SelectedObject;
      propertyItem.CategoryOrder = this.GetCategoryOrder( definition.CategoryValue );

      return propertyItem;
    }

    private int GetCategoryOrder( object categoryValue )
    {
      Debug.Assert( SelectedObject != null );

      if( categoryValue == null )
        return int.MaxValue;

      int order = int.MaxValue;
        object selectedObject = SelectedObject;
        CategoryOrderAttribute[] orderAttributes = ( selectedObject != null )
          ? ( CategoryOrderAttribute[] )selectedObject.GetType().GetCustomAttributes( typeof( CategoryOrderAttribute ), true )
          : new CategoryOrderAttribute[ 0 ];

        var orderAttribute = orderAttributes
          .FirstOrDefault( ( a ) => object.Equals( a.CategoryValue, categoryValue ) );

        if( orderAttribute != null )
        {
          order = orderAttribute.Order;
        }

      return order;
    }




  }
}
