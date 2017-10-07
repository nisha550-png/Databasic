﻿Imports System.Data.Common
Imports Databasic.ActiveRecord

Namespace ActiveRecord
	Partial Public MustInherit Class Entity

		''' <summary>
		''' Create new instance by generic type and set up all called dictionary keys into new instance properties or fields.
		''' </summary>
		''' <typeparam name="TValue">New instance type.</typeparam>
		''' <param name="data">Data with values for new instance properties and fields.</param>
		''' <returns>New instance by generic type with values by second param.</returns>
		Friend Shared Function ToInstance(Of TValue)(data As Dictionary(Of String, Object)) As TValue
			Dim instance As TValue = Activator.CreateInstance(Of TValue)()
			TryCast(instance, Entity).SetUp(data, True)
			Return instance
		End Function
		''' <summary>
		''' Create new instance by generic type and set up all called reader columns with one row at minimal into 
		''' new instance properties or fields. If TResult is primitive type, reader has to return single row and 
		''' single column select result and that result is converted and returned as to primitive value only.
		''' If reader has no rows, Nothing is returned.
		''' </summary>
		''' <typeparam name="TValue">New result class instance type or any primitive type for single row and single column select result.</typeparam>
		''' <param name="reader">Reader with values for new instance properties and fields.</param>
		''' <param name="columnsByDbNames">Optional class members meta info, indexed by database column names.</param>
		''' <returns>New instance as primitive type or as class instance, completed by reader columns with the same names as TActiveRecord type fields/properties.</returns>
		Friend Shared Function ToInstance(Of TValue)(reader As DbDataReader, Optional ByRef columnsByDbNames As Dictionary(Of String, Databasic.MemberInfo) = Nothing) As TValue
			Dim result As Object = Nothing
			If Not TypeOf reader Is DbDataReader Then Return result
			Dim columns As New List(Of String)
			Dim instanceType = GetType(TValue)
			Dim isEntity As Boolean = GetType(Entity).IsAssignableFrom(instanceType)
			Dim descriptableType As Boolean = Entity._isDescriptableType(instanceType)
			If (descriptableType AndAlso Not TypeOf columnsByDbNames Is Dictionary(Of String, Databasic.MemberInfo)) Then
				columnsByDbNames = MetaDescriptor.GetColumnsByDbNames(instanceType)
			End If
			If reader.HasRows() Then
				reader.Read()
				If descriptableType Then
					result = Activator.CreateInstance(Of TValue)()
					'If isEntity Then DirectCast(result, Databasic.ActiveRecord.Entity).InitResource()
					columns = If(columns.Count = 0, ActiveRecord.Entity._getReaderRowColumns(reader), columns)
					ActiveRecord.Entity._readerRowToInstance(
						reader, columns, columnsByDbNames, result, isEntity
					)
				Else
					result = Convert.ChangeType(reader.Item(0), instanceType)
				End If
			End If
			reader.Close()
			Return result
		End Function

	End Class
End Namespace