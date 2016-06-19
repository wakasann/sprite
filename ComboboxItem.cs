/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2016/6/19
 * Time: 17:17
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace CssSprite
{
	/// <summary>
	/// Description of ComboboxItem.
	/// </summary>
	public class ComboboxItem
	{
		public string Value { get; set; }
		public string Text { get; set; }
	    public ComboboxItem(string value, string text)
	    {
	      Value = value;
	      Text  = text;
	    }
	    public override string ToString()
	    {
	      return Text;
	    }
	    
	}
}
