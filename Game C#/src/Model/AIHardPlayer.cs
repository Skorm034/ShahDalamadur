using Microsoft.VisualBasic;using System;using System.Collections;using System.Collections.Generic;using System.Data;using System.Diagnostics;/// <summary>/// AIHardPlayer is a type of player. This AI will know directions of ships/// when it has found 2 ship tiles and will try to destroy that ship. If that ship/// is not destroyed it will shoot the other way. Ship still not destroyed, then/// the AI knows it has hit multiple ships. Then will try to destoy all around tiles/// that have been hit./// </summary>public class AIHardPlayer : AIPlayer{	/// <summary>	/// Target allows the AI to know more things, for example the source of a	/// shot target	/// </summary>	protected class Target	{		private readonly Location _ShotAt;		private readonly Location _Source;		/// <summary>		/// The target shot at		/// </summary>		/// <value>The target shot at</value>		/// <returns>The target shot at</returns>		public Location ShotAt {			get { return _ShotAt; }		}		/// <summary>		/// The source that added this location as a target.		/// </summary>		/// <value>The source that added this location as a target.</value>		/// <returns>The source that added this location as a target.</returns>		public Location Source {			get { return _Source; }		}		internal Target(Location shootat, Location source)		{			_ShotAt = shootat;			_Source = source;		}		/// <summary>		/// If source shot and shootat shot are on the same row then 		/// give a boolean true		/// </summary>		public bool SameRow {			get { return _ShotAt.Row == _Source.Row; }		}		/// <summary>		/// If source shot and shootat shot are on the same column then 		/// give a boolean true 		/// </summary>		public bool SameColumn {			get { return _ShotAt.Column == _Source.Column; }		}	}	/// <summary>	/// Private enumarator for AI states. currently there are two states,	/// the AI can be searching for a ship, or if it has found a ship it will	/// target the same ship	/// </summary>	private enum AIStates	{		/// <summary>		/// The AI is searching for its next target		/// </summary>		Searching,		/// <summary>		/// The AI is trying to target a ship		/// </summary>		TargetingShip,		/// <summary>		/// The AI is locked onto a ship		/// </summary>		HittingShip	}	private AIStates _CurrentState = AIStates.Searching;	private Stack<Target> _Targets = new Stack<Target>();	private List<Target> _LastHit = new List<Target>();	private Target _CurrentTarget;	public AIHardPlayer(BattleShipsGame game) : base(game)	{	}	/// <summary>	/// GenerateCoords will call upon the right methods to generate the appropriate shooting	/// coordinates	/// </summary>	/// <param name="row">the row that will be shot at</param>	/// <param name="column">the column that will be shot at</param>	protected override void GenerateCoords(ref int row, ref int column)	{		do {			_CurrentTarget = null;			//check which state the AI is in and uppon that choose which coordinate generation			//method will be used.			switch (_CurrentState) {			case AIStates.Searching:				SearchCoords(ref row, ref column);				break;			case AIStates.TargetingShip:			case AIStates.HittingShip:				TargetCoords(ref row, ref column);				break;			default:				throw new ApplicationException("AI has gone in an invalid state");			}		} while ((row < 0 || column < 0 || row >= EnemyGrid.Height || column >= EnemyGrid.Width || EnemyGrid.Item(row, column) != TileView.Sea));		//while inside the grid and not a sea tile do the search	}	/// <summary>	/// TargetCoords is used when a ship has been hit and it will try and destroy	/// this ship	/// </summary>	/// <param name="row">row generated around the hit tile</param>	/// <param name="column">column generated around the hit tile</param>	private void TargetCoords(ref int row, ref int column)	{		Target t = null;		t = _Targets.Pop();		row = t.ShotAt.Row;		column = t.ShotAt.Column;		_CurrentTarget = t;	}	/// <summary>	/// SearchCoords will randomly generate shots within the grid as long as its not hit that tile already	/// </summary>	/// <param name="row">the generated row</param>	/// <param name="column">the generated column</param>	private void SearchCoords(ref int row, ref int column)	{		row = _Random.Next(0, EnemyGrid.Height);		column = _Random.Next(0, EnemyGrid.Width);		_CurrentTarget = new Target(new Location(row, column), null);	}	/// <summary>	/// ProcessShot is able to process each shot that is made and call the right methods belonging	/// to that shot. For example, if its a miss = do nothing, if it's a hit = process that hit location	/// </summary>	/// <param name="row">the row that was shot at</param>	/// <param name="col">the column that was shot at</param>	/// <param name="result">the result from that hit</param>	protected override void ProcessShot(int row, int col, AttackResult result)	{		switch (result.Value) {		case ResultOfAttack.Miss:			_CurrentTarget = null;			break;		case ResultOfAttack.Hit:			ProcessHit(row, col);			break;		case ResultOfAttack.Destroyed:			ProcessDestroy(row, col, result.Ship);			break;		case ResultOfAttack.ShotAlready:			throw new ApplicationException("Error in AI");		}		if (_Targets.Count == 0)			_CurrentState = AIStates.Searching;	}	/// <summary>	/// ProcessDetroy is able to process the destroyed ships targets and remove _LastHit targets.	/// It will also call RemoveShotsAround to remove targets that it was going to shoot at	/// </summary>	/// <param name="row">the row that was shot at and destroyed</param>	/// <param name="col">the row that was shot at and destroyed</param>	/// <param name="ship">the row that was shot at and destroyed</param>	private void ProcessDestroy(int row, int col, Ship ship)	{		bool foundOriginal = false;		Location source = default(Location);		Target current = null;		current = _CurrentTarget;		foundOriginal = false;		//i = 1, as we dont have targets from the current hit...		int i = 0;		for (i = 1; i <= ship.Hits - 1; i++) {			if (!foundOriginal) {				source = current.Source;				//Source is nnothing if the ship was originally hit in				// the middle. This then searched forward, rather than				// backward through the list of targets				if (source == null) {					source = current.ShotAt;					foundOriginal = true;				}			} else {				source = current.ShotAt;			}			//find the source in _LastHit			foreach (Target t in _LastHit) {				if ((!foundOriginal && t.ShotAt == source) || (foundOriginal & t.Source == source)) {					current = t;					_LastHit.Remove(t);					break; // TODO: might not be correct. Was : Exit For				}			}			RemoveShotsAround(current.ShotAt);		}	}	/// <summary>	/// RemoveShotsAround will remove targets that belong to the destroyed ship by checking if 	/// the source of the targets belong to the destroyed ship. If they don't put them on a new stack.	/// Then clear the targets stack and move all the targets that still need to be shot at back 	/// onto the targets stack	/// </summary>	/// <param name="toRemove"></param>	private void RemoveShotsAround(Location toRemove)	{		Stack<Target> newStack = new Stack<Target>();		//create a new stack		//check all targets in the _Targets stack		foreach (Target t in _Targets) {			//if the source of the target does not belong to the destroyed ship put them on the newStack			if (!object.ReferenceEquals(t.Source, toRemove))				newStack.Push(t);		}		_Targets.Clear();		//clear the _Targets stack		//for all the targets in the newStack, move them back onto the _Targets stack		foreach (Target t in newStack) {			_Targets.Push(t);		}		//if the _Targets stack is 0 then change the AI's state back to searching		if (_Targets.Count == 0)			_CurrentState = AIStates.Searching;	}	/// <summary>	/// ProcessHit gets the last hit location coordinates and will ask AddTarget to	/// create targets around that location by calling the method four times each time with	/// a new location around the last hit location.	/// It will then set the state of the AI and if it's not Searching or targetingShip then 	/// start ReOrderTargets.	/// </summary>	/// <param name="row"></param>	/// <param name="col"></param>	private void ProcessHit(int row, int col)	{		_LastHit.Add(_CurrentTarget);		//Uses _CurrentTarget as the source		AddTarget(row - 1, col);		AddTarget(row, col - 1);		AddTarget(row + 1, col);		AddTarget(row, col + 1);		if (_CurrentState == AIStates.Searching) {			_CurrentState = AIStates.TargetingShip;		} else {			//either targetting or hitting... both are the same here			_CurrentState = AIStates.HittingShip;			ReOrderTargets();		}	}	/// <summary>	/// ReOrderTargets will optimise the targeting by re-orderin the stack that the targets are in.	/// By putting the most important targets at the top they are the ones that will be shot at first.	/// </summary>	private void ReOrderTargets()	{		//if the ship is lying on the same row, call MoveToTopOfStack to optimise on the row		if (_CurrentTarget.SameRow) {			MoveToTopOfStack(_CurrentTarget.ShotAt.Row, -1);		} else if (_CurrentTarget.SameColumn) {			//else if the ship is lying on the same column, call MoveToTopOfStack to optimise on the column			MoveToTopOfStack(-1, _CurrentTarget.ShotAt.Column);		}	}	/// <summary>	/// MoveToTopOfStack will re-order the stack by checkin the coordinates of each target	/// If they have the right column or row values it will be moved to the _Match stack else 	/// put it on the _NoMatch stack. Then move all the targets from the _NoMatch stack back on the 	/// _Targets stack, these will be at the bottom making them less important. The move all the	/// targets from the _Match stack on the _Targets stack, these will be on the top and will there	/// for be shot at first	/// </summary>	/// <param name="row">the row of the optimisation</param>	/// <param name="column">the column of the optimisation</param>	private void MoveToTopOfStack(int row, int column)	{		Stack<Target> _NoMatch = new Stack<Target>();		Stack<Target> _Match = new Stack<Target>();		Target current = null;		while (_Targets.Count > 0) {			current = _Targets.Pop();			if (current.ShotAt.Row == row || current.ShotAt.Column == column) {				_Match.Push(current);			} else {				_NoMatch.Push(current);			}		}		foreach (Target t in _NoMatch) {			_Targets.Push(t);		}		foreach (Target t in _Match) {			_Targets.Push(t);		}	}	/// <summary>	/// AddTarget will add the targets it will shoot onto a stack	/// </summary>	/// <param name="row">the row of the targets location</param>	/// <param name="column">the column of the targets location</param>	private void AddTarget(int row, int column)	{		if ((row >= 0 && column >= 0 && row < EnemyGrid.Height && column < EnemyGrid.Width && EnemyGrid.Item(row, column) == TileView.Sea)) {			_Targets.Push(new Target(new Location(row, column), _CurrentTarget.ShotAt));		}	}}	 
 u s i n g   S y s t e m . C o l l e c t i o n s ;  
 u s i n g   S y s t e m . C o l l e c t i o n s . G e n e r i c ;  
 u s i n g   S y s t e m . D a t a ;  
 u s i n g   S y s t e m . D i a g n o s t i c s ;  
  
 / / /   < s u m m a r y >  
 / / /   A I H a r d P l a y e r   i s   a   t y p e   o f   p l a y e r .   T h i s   A I   w i l l   k n o w   d i r e c t i o n s   o f   s h i p s  
 / / /   w h e n   i t   h a s   f o u n d   2   s h i p   t i l e s   a n d   w i l l   t r y   t o   d e s t r o y   t h a t   s h i p .   I f   t h a t   s h i p  
 / / /   i s   n o t   d e s t r o y e d   i t   w i l l   s h o o t   t h e   o t h e r   w a y .   S h i p   s t i l l   n o t   d e s t r o y e d ,   t h e n  
 / / /   t h e   A I   k n o w s   i t   h a s   h i t   m u l t i p l e   s h i p s .   T h e n   w i l l   t r y   t o   d e s t o y   a l l   a r o u n d   t i l e s  
 / / /   t h a t   h a v e   b e e n   h i t .  
 / / /   < / s u m m a r y >  
 p u b l i c   c l a s s   A I H a r d P l a y e r   :   A I P l a y e r  
 {  
  
 	 / / /   < s u m m a r y >  
 	 / / /   T a r g e t   a l l o w s   t h e   A I   t o   k n o w   m o r e   t h i n g s ,   f o r   e x a m p l e   t h e   s o u r c e   o f   a  
 	 / / /   s h o t   t a r g e t  
 	 / / /   < / s u m m a r y >  
 	 p r o t e c t e d   c l a s s   T a r g e t  
 	 {  
 	 	 p r i v a t e   r e a d o n l y   L o c a t i o n   _ S h o t A t ;  
  
 	 	 p r i v a t e   r e a d o n l y   L o c a t i o n   _ S o u r c e ;  
 	 	 / / /   < s u m m a r y >  
 	 	 / / /   T h e   t a r g e t   s h o t   a t  
 	 	 / / /   < / s u m m a r y >  
 	 	 / / /   < v a l u e > T h e   t a r g e t   s h o t   a t < / v a l u e >  
 	 	 / / /   < r e t u r n s > T h e   t a r g e t   s h o t   a t < / r e t u r n s >  
 	 	 p u b l i c   L o c a t i o n   S h o t A t   {  
 	 	 	 g e t   {   r e t u r n   _ S h o t A t ;   }  
 	 	 }  
  
 	 	 / / /   < s u m m a r y >  
 	 	 / / /   T h e   s o u r c e   t h a t   a d d e d   t h i s   l o c a t i o n   a s   a   t a r g e t .  
 	 	 / / /   < / s u m m a r y >  
 	 	 / / /   < v a l u e > T h e   s o u r c e   t h a t   a d d e d   t h i s   l o c a t i o n   a s   a   t a r g e t . < / v a l u e >  
 	 	 / / /   < r e t u r n s > T h e   s o u r c e   t h a t   a d d e d   t h i s   l o c a t i o n   a s   a   t a r g e t . < / r e t u r n s >  
 	 	 p u b l i c   L o c a t i o n   S o u r c e   {  
 	 	 	 g e t   {   r e t u r n   _ S o u r c e ;   }  
 	 	 }  
  
 	 	 i n t e r n a l   T a r g e t ( L o c a t i o n   s h o o t a t ,   L o c a t i o n   s o u r c e )  
 	 	 {  
 	 	 	 _ S h o t A t   =   s h o o t a t ;  
 	 	 	 _ S o u r c e   =   s o u r c e ;  
 	 	 }  
  
 	 	 / / /   < s u m m a r y >  
 	 	 / / /   I f   s o u r c e   s h o t   a n d   s h o o t a t   s h o t   a r e   o n   t h e   s a m e   r o w   t h e n    
 	 	 / / /   g i v e   a   b o o l e a n   t r u e  
 	 	 / / /   < / s u m m a r y >  
 	 	 p u b l i c   b o o l   S a m e R o w   {  
 	 	 	 g e t   {   r e t u r n   _ S h o t A t . R o w   = =   _ S o u r c e . R o w ;   }  
 	 	 }  
  
 	 	 / / /   < s u m m a r y >  
 	 	 / / /   I f   s o u r c e   s h o t   a n d   s h o o t a t   s h o t   a r e   o n   t h e   s a m e   c o l u m n   t h e n    
 	 	 / / /   g i v e   a   b o o l e a n   t r u e    
 	 	 / / /   < / s u m m a r y >  
 	 	 p u b l i c   b o o l   S a m e C o l u m n   {  
 	 	 	 g e t   {   r e t u r n   _ S h o t A t . C o l u m n   = =   _ S o u r c e . C o l u m n ;   }  
 	 	 }  
 	 }  
  
 	 / / /   < s u m m a r y >  
 	 / / /   P r i v a t e   e n u m a r a t o r   f o r   A I   s t a t e s .   c u r r e n t l y   t h e r e   a r e   t w o   s t a t e s ,  
 	 / / /   t h e   A I   c a n   b e   s e a r c h i n g   f o r   a   s h i p ,   o r   i f   i t   h a s   f o u n d   a   s h i p   i t   w i l l  
 	 / / /   t a r g e t   t h e   s a m e   s h i p  
 	 / / /   < / s u m m a r y >  
 	 p r i v a t e   e n u m   A I S t a t e s  
 	 {  
 	 	 / / /   < s u m m a r y >  
 	 	 / / /   T h e   A I   i s   s e a r c h i n g   f o r   i t s   n e x t   t a r g e t  
 	 	 / / /   < / s u m m a r y >  
 	 	 S e a r c h i n g ,  
  
 	 	 / / /   < s u m m a r y >  
 	 	 / / /   T h e   A I   i s   t r y i n g   t o   t a r g e t   a   s h i p  
 	 	 / / /   < / s u m m a r y >  
 	 	 T a r g e t i n g S h i p ,  
  
 	 	 / / /   < s u m m a r y >  
 	 	 / / /   T h e   A I   i s   l o c k e d   o n t o   a   s h i p  
 	 	 / / /   < / s u m m a r y >  
 	 	 H i t t i n g S h i p  
 	 }  
  
 	 p r i v a t e   A I S t a t e s   _ C u r r e n t S t a t e   =   A I S t a t e s . S e a r c h i n g ;  
 	 p r i v a t e   S t a c k < T a r g e t >   _ T a r g e t s   =   n e w   S t a c k < T a r g e t > ( ) ;  
 	 p r i v a t e   L i s t < T a r g e t >   _ L a s t H i t   =   n e w   L i s t < T a r g e t > ( ) ;  
  
 	 p r i v a t e   T a r g e t   _ C u r r e n t T a r g e t ;  
 	 p u b l i c   A I H a r d P l a y e r ( B a t t l e S h i p s G a m e   g a m e )   :   b a s e ( g a m e )  
 	 {  
 	 }  
  
 	 / / /   < s u m m a r y >  
 	 / / /   G e n e r a t e C o o r d s   w i l l   c a l l   u p o n   t h e   r i g h t   m e t h o d s   t o   g e n e r a t e   t h e   a p p r o p r i a t e   s h o o t i n g  
 	 / / /   c o o r d i n a t e s  
 	 / / /   < / s u m m a r y >  
 	 / / /   < p a r a m   n a m e = " r o w " > t h e   r o w   t h a t   w i l l   b e   s h o t   a t < / p a r a m >  
 	 / / /   < p a r a m   n a m e = " c o l u m n " > t h e   c o l u m n   t h a t   w i l l   b e   s h o t   a t < / p a r a m >  
 	 p r o t e c t e d   o v e r r i d e   v o i d   G e n e r a t e C o o r d s ( r e f   i n t   r o w ,   r e f   i n t   c o l u m n )  
 	 {  
 	 	 d o   {  
 	 	 	 _ C u r r e n t T a r g e t   =   n u l l ;  
  
 	 	 	 / / c h e c k   w h i c h   s t a t e   t h e   A I   i s   i n   a n d   u p p o n   t h a t   c h o o s e   w h i c h   c o o r d i n a t e   g e n e r a t i o n  
 	 	 	 / / m e t h o d   w i l l   b e   u s e d .  
 	 	 	 s w i t c h   ( _ C u r r e n t S t a t e )   {  
 	 	 	 c a s e   A I S t a t e s . S e a r c h i n g :  
 	 	 	 	 S e a r c h C o o r d s ( r e f   r o w ,   r e f   c o l u m n ) ;  
 	 	 	 	 b r e a k ;  
 	 	 	 c a s e   A I S t a t e s . T a r g e t i n g S h i p :  
 	 	 	 c a s e   A I S t a t e s . H i t t i n g S h i p :  
 	 	 	 	 T a r g e t C o o r d s ( r e f   r o w ,   r e f   c o l u m n ) ;  
 	 	 	 	 b r e a k ;  
 	 	 	 d e f a u l t :  
 	 	 	 	 t h r o w   n e w   A p p l i c a t i o n E x c e p t i o n ( " A I   h a s   g o n e   i n   a n   i n v a l i d   s t a t e " ) ;  
 	 	 	 }  
  
 	 	 }   w h i l e   ( ( r o w   <   0   | |   c o l u m n   <   0   | |   r o w   > =   E n e m y G r i d . H e i g h t   | |   c o l u m n   > =   E n e m y G r i d . W i d t h   | |   E n e m y G r i d . I t e m ( r o w ,   c o l u m n )   ! =   T i l e V i e w . S e a ) ) ;  
 	 	 / / w h i l e   i n s i d e   t h e   g r i d   a n d   n o t   a   s e a   t i l e   d o   t h e   s e a r c h  
 	 }  
  
 	 / / /   < s u m m a r y >  
 	 / / /   T a r g e t C o o r d s   i s   u s e d   w h e n   a   s h i p   h a s   b e e n   h i t   a n d   i t   w i l l   t r y   a n d   d e s t r o y  
 	 / / /   t h i s   s h i p  
 	 / / /   < / s u m m a r y >  
 	 / / /   < p a r a m   n a m e = " r o w " > r o w   g e n e r a t e d   a r o u n d   t h e   h i t   t i l e < / p a r a m >  
 	 / / /   < p a r a m   n a m e = " c o l u m n " > c o l u m n   g e n e r a t e d   a r o u n d   t h e   h i t   t i l e < / p a r a m >  
 	 p r i v a t e   v o i d   T a r g e t C o o r d s ( r e f   i n t   r o w ,   r e f   i n t   c o l u m n )  
 	 {  
 	 	 T a r g e t   t   =   n u l l ;  
 	 	 t   =   _ T a r g e t s . P o p ( ) ;  
  
 	 	 r o w   =   t . S h o t A t . R o w ;  
 	 	 c o l u m n   =   t . S h o t A t . C o l u m n ;  
 	 	 _ C u r r e n t T a r g e t   =   t ;  
 	 }  
  
 	 / / /   < s u m m a r y >  
 	 / / /   S e a r c h C o o r d s   w i l l   r a n d o m l y   g e n e r a t e   s h o t s   w i t h i n   t h e   g r i d   a s   l o n g   a s   i t s   n o t   h i t   t h a t   t i l e   a l r e a d y  
 	 / / /   < / s u m m a r y >  
 	 / / /   < p a r a m   n a m e = " r o w " > t h e   g e n e r a t e d   r o w < / p a r a m >  
 	 / / /   < p a r a m   n a m e = " c o l u m n " > t h e   g e n e r a t e d   c o l u m n < / p a r a m >  
 	 p r i v a t e   v o i d   S e a r c h C o o r d s ( r e f   i n t   r o w ,   r e f   i n t   c o l u m n )  
 	 {  
 	 	 r o w   =   _ R a n d o m . N e x t ( 0 ,   E n e m y G r i d . H e i g h t ) ;  
 	 	 c o l u m n   =   _ R a n d o m . N e x t ( 0 ,   E n e m y G r i d . W i d t h ) ;  
 	 	 _ C u r r e n t T a r g e t   =   n e w   T a r g e t ( n e w   L o c a t i o n ( r o w ,   c o l u m n ) ,   n u l l ) ;  
 	 }  
  
 	 / / /   < s u m m a r y >  
 	 / / /   P r o c e s s S h o t   i s   a b l e   t o   p r o c e s s   e a c h   s h o t   t h a t   i s   m a d e   a n d   c a l l   t h e   r i g h t   m e t h o d s   b e l o n g i n g  
 	 / / /   t o   t h a t   s h o t .   F o r   e x a m p l e ,   i f   i t s   a   m i s s   =   d o   n o t h i n g ,   i f   i t ' s   a   h i t   =   p r o c e s s   t h a t   h i t   l o c a t i o n  
 	 / / /   < / s u m m a r y >  
 	 / / /   < p a r a m   n a m e = " r o w " > t h e   r o w   t h a t   w a s   s h o t   a t < / p a r a m >  
 	 / / /   < p a r a m   n a m e = " c o l " > t h e   c o l u m n   t h a t   w a s   s h o t   a t < / p a r a m >  
 	 / / /   < p a r a m   n a m e = " r e s u l t " > t h e   r e s u l t   f r o m   t h a t   h i t < / p a r a m >  
 	 p r o t e c t e d   o v e r r i d e   v o i d   P r o c e s s S h o t ( i n t   r o w ,   i n t   c o l ,   A t t a c k R e s u l t   r e s u l t )  
 	 {  
 	 	 s w i t c h   ( r e s u l t . V a l u e )   {  
 	 	 c a s e   R e s u l t O f A t t a c k . M i s s :  
 	 	 	 	 _ C u r r e n t T a r g e t   =   n u l l ;  
 	 	 	 	 b r e a k ;  
 	 	 c a s e   R e s u l t O f A t t a c k . H i t :  
 	 	 	 P r o c e s s H i t ( r o w ,   c o l ) ;  
 	 	 	 b r e a k ;  
 	 	 c a s e   R e s u l t O f A t t a c k . D e s t r o y e d :  
 	 	 	 P r o c e s s D e s t r o y ( r o w ,   c o l ,   r e s u l t . S h i p ) ;  
 	 	 	 b r e a k ;  
 	 	 c a s e   R e s u l t O f A t t a c k . S h o t A l r e a d y :  
 	 	 	 t h r o w   n e w   A p p l i c a t i o n E x c e p t i o n ( " E r r o r   i n   A I " ) ;  
 	 	 }  
  
 	 	 i f   ( _ T a r g e t s . C o u n t   = =   0 )  
 	 	 	 _ C u r r e n t S t a t e   =   A I S t a t e s . S e a r c h i n g ;  
 	 }  
  
 	 / / /   < s u m m a r y >  
 	 / / /   P r o c e s s D e t r o y   i s   a b l e   t o   p r o c e s s   t h e   d e s t r o y e d   s h i p s   t a r g e t s   a n d   r e m o v e   _ L a s t H i t   t a r g e t s .  
 	 / / /   I t   w i l l   a l s o   c a l l   R e m o v e S h o t s A r o u n d   t o   r e m o v e   t a r g e t s   t h a t   i t   w a s   g o i n g   t o   s h o o t   a t  
 	 / / /   < / s u m m a r y >  
 	 / / /   < p a r a m   n a m e = " r o w " > t h e   r o w   t h a t   w a s   s h o t   a t   a n d   d e s t r o y e d < / p a r a m >  
 	 / / /   < p a r a m   n a m e = " c o l " > t h e   r o w   t h a t   w a s   s h o t   a t   a n d   d e s t r o y e d < / p a r a m >  
 	 / / /   < p a r a m   n a m e = " s h i p " > t h e   r o w   t h a t   w a s   s h o t   a t   a n d   d e s t r o y e d < / p a r a m >  
 	 p r i v a t e   v o i d   P r o c e s s D e s t r o y ( i n t   r o w ,   i n t   c o l ,   S h i p   s h i p )  
 	 {  
 	 	 b o o l   f o u n d O r i g i n a l   =   f a l s e ;  
 	 	 L o c a t i o n   s o u r c e   =   d e f a u l t ( L o c a t i o n ) ;  
 	 	 T a r g e t   c u r r e n t   =   n u l l ;  
 	 	 c u r r e n t   =   _ C u r r e n t T a r g e t ;  
  
 	 	 f o u n d O r i g i n a l   =   f a l s e ;  
  
 	 	 / / i   =   1 ,   a s   w e   d o n t   h a v e   t a r g e t s   f r o m   t h e   c u r r e n t   h i t . . .  
 	 	 i n t   i   =   0 ;  
  
 	 	 f o r   ( i   =   1 ;   i   < =   s h i p . H i t s   -   1 ;   i + + )   {  
 	 	 	 i f   ( ! f o u n d O r i g i n a l )   {  
 	 	 	 	 s o u r c e   =   c u r r e n t . S o u r c e ;  
 	 	 	 	 / / S o u r c e   i s   n n o t h i n g   i f   t h e   s h i p   w a s   o r i g i n a l l y   h i t   i n  
 	 	 	 	 / /   t h e   m i d d l e .   T h i s   t h e n   s e a r c h e d   f o r w a r d ,   r a t h e r   t h a n  
 	 	 	 	 / /   b a c k w a r d   t h r o u g h   t h e   l i s t   o f   t a r g e t s  
 	 	 	 	 i f   ( s o u r c e   = =   n u l l )   {  
 	 	 	 	 	 s o u r c e   =   c u r r e n t . S h o t A t ;  
 	 	 	 	 	 f o u n d O r i g i n a l   =   t r u e ;  
 	 	 	 	 }  
 	 	 	 }   e l s e   {  
 	 	 	 	 s o u r c e   =   c u r r e n t . S h o t A t ;  
 	 	 	 }  
  
 	 	 	 / / f i n d   t h e   s o u r c e   i n   _ L a s t H i t  
 	 	 	 f o r e a c h   ( T a r g e t   t   i n   _ L a s t H i t )   {  
 	 	 	 	 i f   ( ( ! f o u n d O r i g i n a l   & &   t . S h o t A t   = =   s o u r c e )   | |   ( f o u n d O r i g i n a l   &   t . S o u r c e   = =   s o u r c e ) )   {  
 	 	 	 	 	 c u r r e n t   =   t ;  
 	 	 	 	 	 _ L a s t H i t . R e m o v e ( t ) ;  
 	 	 	 	 	 b r e a k ;   / /   T O D O :   m i g h t   n o t   b e   c o r r e c t .   W a s   :   E x i t   F o r  
 	 	 	 	 }  
 	 	 	 }  
  
 	 	 	 R e m o v e S h o t s A r o u n d ( c u r r e n t . S h o t A t ) ;  
 	 	 }  
 	 }  
  
 	 / / /   < s u m m a r y >  
 	 / / /   R e m o v e S h o t s A r o u n d   w i l l   r e m o v e   t a r g e t s   t h a t   b e l o n g   t o   t h e   d e s t r o y e d   s h i p   b y   c h e c k i n g   i f    
 	 / / /   t h e   s o u r c e   o f   t h e   t a r g e t s   b e l o n g   t o   t h e   d e s t r o y e d   s h i p .   I f   t h e y   d o n ' t   p u t   t h e m   o n   a   n e w   s t a c k .  
 	 / / /   T h e n   c l e a r   t h e   t a r g e t s   s t a c k   a n d   m o v e   a l l   t h e   t a r g e t s   t h a t   s t i l l   n e e d   t o   b e   s h o t   a t   b a c k    
 	 / / /   o n t o   t h e   t a r g e t s   s t a c k  
 	 / / /   < / s u m m a r y >  
 	 / / /   < p a r a m   n a m e = " t o R e m o v e " > < / p a r a m >  
 	 p r i v a t e   v o i d   R e m o v e S h o t s A r o u n d ( L o c a t i o n   t o R e m o v e )  
 	 {  
 	 	 S t a c k < T a r g e t >   n e w S t a c k   =   n e w   S t a c k < T a r g e t > ( ) ;  
 	 	 / / c r e a t e   a   n e w   s t a c k  
  
 	 	 / / c h e c k   a l l   t a r g e t s   i n   t h e   _ T a r g e t s   s t a c k  
  
 	 	 f o r e a c h   ( T a r g e t   t   i n   _ T a r g e t s )   {  
 	 	 	 / / i f   t h e   s o u r c e   o f   t h e   t a r g e t   d o e s   n o t   b e l o n g   t o   t h e   d e s t r o y e d   s h i p   p u t   t h e m   o n   t h e   n e w S t a c k  
 	 	 	 i f   ( ! o b j e c t . R e f e r e n c e E q u a l s ( t . S o u r c e ,   t o R e m o v e ) )  
 	 	 	 	 n e w S t a c k . P u s h ( t ) ;  
 	 	 }  
  
 	 	 _ T a r g e t s . C l e a r ( ) ;  
 	 	 / / c l e a r   t h e   _ T a r g e t s   s t a c k  
  
 	 	 / / f o r   a l l   t h e   t a r g e t s   i n   t h e   n e w S t a c k ,   m o v e   t h e m   b a c k   o n t o   t h e   _ T a r g e t s   s t a c k  
 	 	 f o r e a c h   ( T a r g e t   t   i n   n e w S t a c k )   {  
 	 	 	 _ T a r g e t s . P u s h ( t ) ;  
 	 	 }  
  
 	 	 / / i f   t h e   _ T a r g e t s   s t a c k   i s   0   t h e n   c h a n g e   t h e   A I ' s   s t a t e   b a c k   t o   s e a r c h i n g  
 	 	 i f   ( _ T a r g e t s . C o u n t   = =   0 )  
 	 	 	 _ C u r r e n t S t a t e   =   A I S t a t e s . S e a r c h i n g ;  
 	 }  
  
 	 / / /   < s u m m a r y >  
 	 / / /   P r o c e s s H i t   g e t s   t h e   l a s t   h i t   l o c a t i o n   c o o r d i n a t e s   a n d   w i l l   a s k   A d d T a r g e t   t o  
 	 / / /   c r e a t e   t a r g e t s   a r o u n d   t h a t   l o c a t i o n   b y   c a l l i n g   t h e   m e t h o d   f o u r   t i m e s   e a c h   t i m e   w i t h  
 	 / / /   a   n e w   l o c a t i o n   a r o u n d   t h e   l a s t   h i t   l o c a t i o n .  
 	 / / /   I t   w i l l   t h e n   s e t   t h e   s t a t e   o f   t h e   A I   a n d   i f   i t ' s   n o t   S e a r c h i n g   o r   t a r g e t i n g S h i p   t h e n    
 	 / / /   s t a r t   R e O r d e r T a r g e t s .  
 	 / / /   < / s u m m a r y >  
 	 / / /   < p a r a m   n a m e = " r o w " > < / p a r a m >  
 	 / / /   < p a r a m   n a m e = " c o l " > < / p a r a m >  
 	 p r i v a t e   v o i d   P r o c e s s H i t ( i n t   r o w ,   i n t   c o l )  
 	 {  
 	 	 _ L a s t H i t . A d d ( _ C u r r e n t T a r g e t ) ;  
  
 	 	 / / U s e s   _ C u r r e n t T a r g e t   a s   t h e   s o u r c e  
 	 	 A d d T a r g e t ( r o w   -   1 ,   c o l ) ;  
 	 	 A d d T a r g e t ( r o w ,   c o l   -   1 ) ;  
 	 	 A d d T a r g e t ( r o w   +   1 ,   c o l ) ;  
 	 	 A d d T a r g e t ( r o w ,   c o l   +   1 ) ;  
  
 	 	 i f   ( _ C u r r e n t S t a t e   = =   A I S t a t e s . S e a r c h i n g )   {  
 	 	 	 _ C u r r e n t S t a t e   =   A I S t a t e s . T a r g e t i n g S h i p ;  
 	 	 }   e l s e   {  
 	 	 	 / / e i t h e r   t a r g e t t i n g   o r   h i t t i n g . . .   b o t h   a r e   t h e   s a m e   h e r e  
 	 	 	 _ C u r r e n t S t a t e   =   A I S t a t e s . H i t t i n g S h i p ;  
  
 	 	 	 R e O r d e r T a r g e t s ( ) ;  
 	 	 }  
 	 }  
  
 	 / / /   < s u m m a r y >  
 	 / / /   R e O r d e r T a r g e t s   w i l l   o p t i m i s e   t h e   t a r g e t i n g   b y   r e - o r d e r i n   t h e   s t a c k   t h a t   t h e   t a r g e t s   a r e   i n .  
 	 / / /   B y   p u t t i n g   t h e   m o s t   i m p o r t a n t   t a r g e t s   a t   t h e   t o p   t h e y   a r e   t h e   o n e s   t h a t   w i l l   b e   s h o t   a t   f i r s t .  
 	 / / /   < / s u m m a r y >  
  
 	 p r i v a t e   v o i d   R e O r d e r T a r g e t s ( )  
 	 {  
 	 	 / / i f   t h e   s h i p   i s   l y i n g   o n   t h e   s a m e   r o w ,   c a l l   M o v e T o T o p O f S t a c k   t o   o p t i m i s e   o n   t h e   r o w  
 	 	 i f   ( _ C u r r e n t T a r g e t . S a m e R o w )   {  
 	 	 	 M o v e T o T o p O f S t a c k ( _ C u r r e n t T a r g e t . S h o t A t . R o w ,   - 1 ) ;  
 	 	 }   e l s e   i f   ( _ C u r r e n t T a r g e t . S a m e C o l u m n )   {  
 	 	 	 / / e l s e   i f   t h e   s h i p   i s   l y i n g   o n   t h e   s a m e   c o l u m n ,   c a l l   M o v e T o T o p O f S t a c k   t o   o p t i m i s e   o n   t h e   c o l u m n  
 	 	 	 M o v e T o T o p O f S t a c k ( - 1 ,   _ C u r r e n t T a r g e t . S h o t A t . C o l u m n ) ;  
 	 	 }  
 	 }  
  
 	 / / /   < s u m m a r y >  
 	 / / /   M o v e T o T o p O f S t a c k   w i l l   r e - o r d e r   t h e   s t a c k   b y   c h e c k i n   t h e   c o o r d i n a t e s   o f   e a c h   t a r g e t  
 	 / / /   I f   t h e y   h a v e   t h e   r i g h t   c o l u m n   o r   r o w   v a l u e s   i t   w i l l   b e   m o v e d   t o   t h e   _ M a t c h   s t a c k   e l s e    
 	 / / /   p u t   i t   o n   t h e   _ N o M a t c h   s t a c k .   T h e n   m o v e   a l l   t h e   t a r g e t s   f r o m   t h e   _ N o M a t c h   s t a c k   b a c k   o n   t h e    
 	 / / /   _ T a r g e t s   s t a c k ,   t h e s e   w i l l   b e   a t   t h e   b o t t o m   m a k i n g   t h e m   l e s s   i m p o r t a n t .   T h e   m o v e   a l l   t h e  
 	 / / /   t a r g e t s   f r o m   t h e   _ M a t c h   s t a c k   o n   t h e   _ T a r g e t s   s t a c k ,   t h e s e   w i l l   b e   o n   t h e   t o p   a n d   w i l l   t h e r e  
 	 / / /   f o r   b e   s h o t   a t   f i r s t  
 	 / / /   < / s u m m a r y >  
 	 / / /   < p a r a m   n a m e = " r o w " > t h e   r o w   o f   t h e   o p t i m i s a t i o n < / p a r a m >  
 	 / / /   < p a r a m   n a m e = " c o l u m n " > t h e   c o l u m n   o f   t h e   o p t i m i s a t i o n < / p a r a m >  
 	 p r i v a t e   v o i d   M o v e T o T o p O f S t a c k ( i n t   r o w ,   i n t   c o l u m n )  
 	 {  
 	 	 S t a c k < T a r g e t >   _ N o M a t c h   =   n e w   S t a c k < T a r g e t > ( ) ;  
 	 	 S t a c k < T a r g e t >   _ M a t c h   =   n e w   S t a c k < T a r g e t > ( ) ;  
  
 	 	 T a r g e t   c u r r e n t   =   n u l l ;  
  
 	 	 w h i l e   ( _ T a r g e t s . C o u n t   >   0 )   {  
 	 	 	 c u r r e n t   =   _ T a r g e t s . P o p ( ) ;  
 	 	 	 i f   ( c u r r e n t . S h o t A t . R o w   = =   r o w   | |   c u r r e n t . S h o t A t . C o l u m n   = =   c o l u m n )   {  
 	 	 	 	 _ M a t c h . P u s h ( c u r r e n t ) ;  
 	 	 	 }   e l s e   {  
 	 	 	 	 _ N o M a t c h . P u s h ( c u r r e n t ) ;  
 	 	 	 }  
 	 	 }  
  
 	 	 f o r e a c h   ( T a r g e t   t   i n   _ N o M a t c h )   {  
 	 	 	 _ T a r g e t s . P u s h ( t ) ;  
 	 	 }  
 	 	 f o r e a c h   ( T a r g e t   t   i n   _ M a t c h )   {  
 	 	 	 _ T a r g e t s . P u s h ( t ) ;  
 	 	 }  
 	 }  
  
 	 / / /   < s u m m a r y >  
 	 / / /   A d d T a r g e t   w i l l   a d d   t h e   t a r g e t s   i t   w i l l   s h o o t   o n t o   a   s t a c k  
 	 / / /   < / s u m m a r y >  
 	 / / /   < p a r a m   n a m e = " r o w " > t h e   r o w   o f   t h e   t a r g e t s   l o c a t i o n < / p a r a m >  
 	 / / /   < p a r a m   n a m e = " c o l u m n " > t h e   c o l u m n   o f   t h e   t a r g e t s   l o c a t i o n < / p a r a m >  
  
 	 p r i v a t e   v o i d   A d d T a r g e t ( i n t   r o w ,   i n t   c o l u m n )  
 	 {  
  
 	 	 i f   ( ( r o w   > =   0   & &   c o l u m n   > =   0   & &   r o w   <   E n e m y G r i d . H e i g h t   & &   c o l u m n   <   E n e m y G r i d . W i d t h   & &   E n e m y G r i d . I t e m ( r o w ,   c o l u m n )   = =   T i l e V i e w . S e a ) )   {  
 	 	 	 _ T a r g e t s . P u s h ( n e w   T a r g e t ( n e w   L o c a t i o n ( r o w ,   c o l u m n ) ,   _ C u r r e n t T a r g e t . S h o t A t ) ) ;  
 	 	 }  
 	 }  
  
 }  
  
 / / = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =  
 / / S e r v i c e   p r o v i d e d   b y   T e l e r i k   ( w w w . t e l e r i k . c o m )  
 / / C o n v e r s i o n   p o w e r e d   b y   N R e f a c t o r y .  
 / / T w i t t e r :   @ t e l e r i k  
 / / F a c e b o o k :   f a c e b o o k . c o m / t e l e r i k  
 / / = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =  
 