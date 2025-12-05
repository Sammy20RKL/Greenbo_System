/**
 * Dashboard Analytics
 */

'use strict';

document.addEventListener('DOMContentLoaded', function (e) {
  let cardColor, headingColor, legendColor, labelColor, borderColor, fontFamily;

  // Get theme colors from config
  cardColor = config.colors.cardColor;
  headingColor = config.colors.headingColor;
  legendColor = config.colors.bodyColor;
  labelColor = config.colors.textMuted;
  borderColor = config.colors.borderColor;
  fontFamily = config.fontFamily;

  // Project Analytics Chart - Horizontal Bar Chart
  // --------------------------------------------------------------------
  const projectAnalyticsChartEl = document.querySelector('#projectAnalyticsChart');
  const analyticsOptions = {
    series: [{
      name: 'Projects',
      data: [8, 12, 15, 18, 20, 16, 14, 18, 22, 20, 18, 15]
    }],
    chart: {
      type: 'bar',
      height: 270,
      toolbar: { show: false }
    },
    plotOptions: {
      bar: {
        horizontal: false,
        barHeight: '60%',
        borderRadius: 6,
        borderRadiusApplication: 'end'
      }
    },
    colors: ['#2d6f4e'],
    dataLabels: {
      enabled: false
    },
    xaxis: {
      categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
      labels: {
        style: {
          fontSize: '11px',
          fontFamily: fontFamily,
          colors: labelColor
        }
      },
      axisBorder: {
        show: false
      },
      axisTicks: {
        show: false
      }
    },
    yaxis: {
      labels: {
        style: {
          fontSize: '12px',
          fontFamily: fontFamily,
          colors: labelColor
        }
      }
    },
    grid: {
      borderColor: borderColor,
      strokeDashArray: 4,
      xaxis: {
        lines: {
          show: true
        }
      },
      yaxis: {
        lines: {
          show: false
        }
      },
      padding: {
        top: 10,
        right: 10,
        bottom: 10,
        left: 10
      }
    },
    states: {
      hover: {
        filter: {
          type: 'lighten',
          value: 0.1
        }
      },
      active: {
        filter: {
          type: 'darken',
          value: 0.1
        }
      }
    }
  };

  if (typeof projectAnalyticsChartEl !== 'undefined' && projectAnalyticsChartEl !== null) {
    const analyticsChart = new ApexCharts(projectAnalyticsChartEl, analyticsOptions);
    analyticsChart.render();
  }

  // Project Progress Chart - Donut Chart
  // --------------------------------------------------------------------
  const projectProgressChartEl = document.querySelector('#projectProgressChart');
  const progressOptions = {
    series: [41, 35, 24],
    chart: {
      type: 'donut',
      height: 200
    },
    labels: ['Completed', 'In Progress', 'Pending'],
    colors: ['#2d6f4e', '#f0ad4e', '#5bc0de'],
    plotOptions: {
      pie: {
        donut: {
          size: '65%',
          labels: {
            show: true,
            name: {
              show: true,
              fontSize: '13px',
              fontFamily: fontFamily,
              color: legendColor,
              offsetY: 20
            },
            value: {
              show: true,
              fontSize: '32px',
              fontFamily: fontFamily,
              fontWeight: 700,
              color: headingColor,
              offsetY: -20,
              formatter: function (val) {
                return parseInt(val) + '%';
              }
            },
            total: {
              show: true,
              label: 'Progress',
              fontSize: '13px',
              fontFamily: fontFamily,
              color: legendColor,
              formatter: function (w) {
                return '41%';
              }
            }
          }
        }
      }
    },
    legend: {
      show: false
    },
    dataLabels: {
      enabled: false
    },
    stroke: {
      width: 3,
      colors: [cardColor]
    },
    states: {
      hover: {
        filter: {
          type: 'lighten',
          value: 0.1
        }
      },
      active: {
        filter: {
          type: 'darken',
          value: 0.1
        }
      }
    },
    responsive: [
      {
        breakpoint: 1400,
        options: {
          chart: {
            height: 180
          }
        }
      }
    ]
  };

  if (typeof projectProgressChartEl !== 'undefined' && projectProgressChartEl !== null) {
    const progressChart = new ApexCharts(projectProgressChartEl, progressOptions);
    progressChart.render();
  }

  // Timer Functionality (Optional)
  // --------------------------------------------------------------------
  let timerInterval;
  let timerSeconds = 5048; // 01:24:08 = 5048 seconds
  let isTimerRunning = false;

  const timerDisplay = document.querySelector('.timer-display');
  const pauseBtn = document.querySelector('.timer-btn:first-child');
  const stopBtn = document.querySelector('.timer-btn:last-child');

  function updateTimerDisplay() {
    const hours = Math.floor(timerSeconds / 3600);
    const minutes = Math.floor((timerSeconds % 3600) / 60);
    const seconds = timerSeconds % 60;

    if (timerDisplay) {
      timerDisplay.textContent =
        String(hours).padStart(2, '0') + ':' +
        String(minutes).padStart(2, '0') + ':' +
        String(seconds).padStart(2, '0');
    }
  }

  function startTimer() {
    if (!isTimerRunning) {
      isTimerRunning = true;
      timerInterval = setInterval(function () {
        timerSeconds++;
        updateTimerDisplay();
      }, 1000);

      if (pauseBtn) {
        pauseBtn.querySelector('i').className = 'bx bx-pause';
      }
    }
  }

  function pauseTimer() {
    if (isTimerRunning) {
      clearInterval(timerInterval);
      isTimerRunning = false;

      if (pauseBtn) {
        pauseBtn.querySelector('i').className = 'bx bx-play';
      }
    } else {
      startTimer();
    }
  }

  function stopTimer() {
    clearInterval(timerInterval);
    isTimerRunning = false;
    timerSeconds = 0;
    updateTimerDisplay();

    if (pauseBtn) {
      pauseBtn.querySelector('i').className = 'bx bx-play';
    }
  }

  // Timer event listeners
  if (pauseBtn) {
    pauseBtn.addEventListener('click', pauseTimer);
  }

  if (stopBtn) {
    stopBtn.addEventListener('click', stopTimer);
  }

  // Initialize timer display
  updateTimerDisplay();
});
