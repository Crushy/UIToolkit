using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class UIAbstractContainer : UIObject, IPositionable
{
	public enum UILayoutType { Horizontal, Vertical };
	private UILayoutType _layoutType;
	public UILayoutType layoutType { get { return _layoutType; } set { _layoutType = value; layoutChildren(); } } // relayout when layoutType changes
	
	protected int _spacing; // spacing is the space between each object
	public int spacing { get { return _spacing; } set { _spacing = value; layoutChildren(); } } // relayout when spacing changes
	
	public UIEdgeInsets _edgeInsets; // pixel padding insets for top, left, bottom and right
	public UIEdgeInsets edgeInsets { get { return _edgeInsets; } set { _edgeInsets = value; layoutChildren(); } } // relayout when edgeInsets changes
	
	protected float _width;
	public float width { get { return _width; } }

	protected float _height;
	public float height { get { return _height; } }
	
	protected List<UISprite> _children = new List<UISprite>();
	private bool _suspendUpdates; // when true, layoutChildren will do nothing



	/// <summary>
	/// We need the layout type set from the getgo so we can lay things out properly
	/// </summary>
	public UIAbstractContainer( UILayoutType layoutType )
	{
		_layoutType = layoutType;
	}


	/// <summary>
	/// Adds a UISprite to the container and sets it to lay itself out
	/// </summary>
	public virtual void addChild( params UISprite[] children )
	{
		foreach( var child in children )
		{
			child.parentUIObject = this;
			_children.Add( child );
		}
		
		layoutChildren();
	}
	

	/// <summary>
	/// Removes a child from the container and optionally from it's manager.  If it is removed from
	/// it's manager it is no longer in existance so be sure to null out any references to it.
	/// </summary>
	public void removeChild( UISprite child, bool removeFromManager )
	{
#if UNITY_EDITOR
		// sanity check while we are in the editor
		if( !_children.Contains( child ) )
			throw new System.Exception( "could not find child in UIAbstractContainer: " + child );
#endif
		_children.Remove( child );
		layoutChildren();
		
		if( removeFromManager )
			child.manager.removeElement( child );
	}


	/// <summary>
	/// Call this when changing multiple properties at once that result in autolayout.  Must be
	/// paired with a call to endUpdates!
	/// </summary>
	public void beginUpdates()
	{
		_suspendUpdates = true;
	}
	
	
	/// <summary>
	/// Commits any update made after beginUpdates was called
	/// </summary>
	public void endUpdates()
	{
		_suspendUpdates = false;
		layoutChildren();
	}
	

	/// <summary>
	/// Responsible for laying out the child UISprites
	/// </summary>
	protected virtual void layoutChildren()
	{
		if( _suspendUpdates )
			return;

		// start with the insets, then add each object + spacing then end with insets
		_width = _edgeInsets.left;
		_height = _edgeInsets.top;
			
		if( _layoutType == UIAbstractContainer.UILayoutType.Horizontal )
		{
			var i = 0;
			var lastIndex = _children.Count;
			foreach( var item in _children )
			{
				// we add spacing for all but the first and last
				if( i != 0 && i != lastIndex )
					_width += _spacing;
				
				var yPos = item.gameObjectOriginInCenter ? -item.height / 2 : 0;
				var xPosModifier = item.gameObjectOriginInCenter ? item.width / 2 : 0;
				item.localPosition = new Vector3( _width + xPosModifier, _edgeInsets.top + yPos, item.position.z );

				// all items get their width added
				_width += item.width;
				
				// height will just be the height of the tallest item
				if( _height < item.height )
					_height = item.height;
				
				i++;
			}
		}
		else // vertical alignment
		{
			var i = 0;
			var lastIndex = _children.Count;
			foreach( var item in _children )
			{
				// we add spacing for all but the first and last
				if( i != 0 && i != lastIndex )
					_height += _spacing;
				
				var xPos = item.gameObjectOriginInCenter ? item.width / 2 : 0;
				var yPosModifier = item.gameObjectOriginInCenter ? item.height / 2 : 0;
				
				item.localPosition = new Vector3( _edgeInsets.left + xPos, -( _height + yPosModifier ), item.position.z );

				// all items get their height added
				_height += item.height;
				
				// width will just be the width of the widest item
				if( _width < item.width )
					_width = item.width;
				
				i++;
			}
		}
		
		// add the right and bottom edge inset to finish things off
		_width += _edgeInsets.right;
		_height += _edgeInsets.bottom;
	}


	public override void transformChanged()
	{
		Debug.Log( "transformChanged transformChanged transformChanged transformChanged" );
		layoutChildren();
	}

}
