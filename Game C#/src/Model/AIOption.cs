using Microsoft.VisualBasic;using System;using System.Collections;using System.Collections.Generic;using System.Data;using System.Diagnostics;/// <summary>/// The different AI levels./// </summary>public enum AIOption{	/// <summary>	/// Easy, total random shooting	/// </summary>	Easy,	/// <summary>	/// Medium, marks squares around hits	/// </summary>	Medium,	/// <summary>	/// As medium, but removes shots once it misses	/// </summary>	Hard}//=======================================================//Service provided by Telerik (www.telerik.com)//Conversion powered by NRefactory.//Twitter: @telerik//Facebook: facebook.com/telerik//======================================================= 
 u s i n g   S y s t e m . C o l l e c t i o n s ;  
 u s i n g   S y s t e m . C o l l e c t i o n s . G e n e r i c ;  
 u s i n g   S y s t e m . D a t a ;  
 u s i n g   S y s t e m . D i a g n o s t i c s ;  
  
 / / /   < s u m m a r y >  
 / / /   T h e   d i f f e r e n t   A I   l e v e l s .  
 / / /   < / s u m m a r y >  
 p u b l i c   e n u m   A I O p t i o n  
 {  
 	 / / /   < s u m m a r y >  
 	 / / /   E a s y ,   t o t a l   r a n d o m   s h o o t i n g  
 	 / / /   < / s u m m a r y >  
 	 E a s y ,  
  
 	 / / /   < s u m m a r y >  
 	 / / /   M e d i u m ,   m a r k s   s q u a r e s   a r o u n d   h i t s  
 	 / / /   < / s u m m a r y >  
 	 M e d i u m ,  
  
 	 / / /   < s u m m a r y >  
 	 / / /   A s   m e d i u m ,   b u t   r e m o v e s   s h o t s   o n c e   i t   m i s s e s  
 	 / / /   < / s u m m a r y >  
 	 H a r d  
 }  
 