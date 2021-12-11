using System;

public struct GameDate {
	private DateTime _dateTime;
	public int DayTicks;

	public GameDate(int dayTicks) {
		_dateTime = new DateTime(1, 1, 1).AddDays(dayTicks);
		this.DayTicks = dayTicks;
	}

	public GameDate NextDay() {
		return new GameDate(DayTicks + 1);
	}

	public bool isFirstOfMonth { get { return _dateTime.Day == 1; } }
	public bool isFirstOfWeek { get { return _dateTime.DayOfWeek == DayOfWeek.Monday; } }

	public int DayOfYear {
		get {
			return _dateTime.DayOfYear;
		}
	}

	public override string ToString() {
		return $"{_dateTime.ToString("dddd")}, {_dateTime.ToString("MMMM d")}, year {_dateTime.Year}";
	}
}
