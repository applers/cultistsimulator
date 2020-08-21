﻿namespace UIWidgets
{
	using System;
	using UIWidgets.Attributes;
	using UnityEngine;

	/// <summary>
	/// Multiple dates support for Calendar.
	/// </summary>
	[RequireComponent(typeof(CalendarBase))]
	public class CalendarMultipleDates : MonoBehaviour
	{
		/// <summary>
		/// Date comparer.
		/// </summary>
		protected struct DateComparer
		{
			readonly CalendarBase Calendar;
			readonly DateTime Date;

			/// <summary>
			/// Initializes a new instance of the <see cref="DateComparer"/> struct.
			/// </summary>
			/// <param name="calendar">Calender.</param>
			/// <param name="date">Date to compare with.</param>
			public DateComparer(CalendarBase calendar, DateTime date)
			{
				Calendar = calendar;
				Date = date;
			}

			/// <summary>
			/// Check is date is same day.
			/// </summary>
			/// <param name="date">Date.</param>
			/// <returns>true if date represents the same day; otherwise false.</returns>
			public bool IsSameDay(DateTime date)
			{
				return Calendar.IsSameDay(Date, date);
			}
		}

		/// <summary>
		/// Data source.
		/// </summary>
		protected ObservableList<DateTime> dataSource;

		/// <summary>
		/// Gets or sets the data source.
		/// </summary>
		/// <value>The data source.</value>
		[DataBindField]
		public virtual ObservableList<DateTime> DataSource
		{
			get
			{
				if (dataSource == null)
				{
					dataSource = new ObservableList<DateTime>();
					dataSource.OnChange += CollectionChanged;
				}

				return dataSource;
			}

			set
			{
				if (dataSource != value)
				{
					if (dataSource != null)
					{
						dataSource.OnChange -= CollectionChanged;
					}

					Init();

					dataSource = value;

					if (dataSource != null)
					{
						dataSource.OnChange += CollectionChanged;
					}

					CollectionChanged();
				}
			}
		}

		CalendarBase currentCalendar;

		/// <summary>
		/// Current calendar.
		/// </summary>
		public CalendarBase CurrentCalendar
		{
			get
			{
				if (currentCalendar == null)
				{
					currentCalendar = GetComponent<CalendarBase>();
				}

				return currentCalendar;
			}
		}

		bool isInited;

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start()
		{
			Init();
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		public virtual void Init()
		{
			if (isInited)
			{
				return;
			}

			CurrentCalendar.OnDateClick.AddListener(ToggleDate);

			isInited = true;

			CollectionChanged();
		}

		/// <summary>
		/// Toggle date state.
		/// </summary>
		/// <param name="date">Date.</param>
		public virtual void ToggleDate(DateTime date)
		{
			if (IsSelected(date))
			{
				DataSource.RemoveAll(new DateComparer(CurrentCalendar, date).IsSameDay);
			}
			else
			{
				DataSource.Add(date);
			}
		}

		/// <summary>
		/// Select date.
		/// </summary>
		/// <param name="date">Date.</param>
		public virtual void SelectDate(DateTime date)
		{
			if (!IsSelected(date))
			{
				DataSource.Add(date);
			}
		}

		/// <summary>
		/// Toggle date state.
		/// </summary>
		/// <param name="date">Date.</param>
		public virtual void DeselectDate(DateTime date)
		{
			if (IsSelected(date))
			{
				DataSource.RemoveAll(new DateComparer(CurrentCalendar, date).IsSameDay);
			}
		}

		/// <summary>
		/// Is date selected?
		/// </summary>
		/// <param name="date">Date.</param>
		/// <returns>true if date selected; otherwise false.</returns>
		public virtual bool IsSelected(DateTime date)
		{
			foreach (var selected_date in DataSource)
			{
				if (CurrentCalendar.IsSameDay(date, selected_date))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Update calendar.
		/// </summary>
		protected virtual void CollectionChanged()
		{
			CurrentCalendar.UpdateCalendar();
		}

		/// <summary>
		/// Process destroy event.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (dataSource != null)
			{
				dataSource.OnChange -= CollectionChanged;
			}
		}
	}
}