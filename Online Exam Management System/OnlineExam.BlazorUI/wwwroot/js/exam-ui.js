window.examUi = {
  confetti: () => {
    if (window.confetti) {
      window.confetti({ particleCount: 140, spread: 80, origin: { y: 0.72 } });
    }
  },
  copyText: async (text) => navigator.clipboard.writeText(text),
  chart: (id, type, labels, values, accent) => {
    const canvas = document.getElementById(id);
    if (!canvas || !window.Chart) return;
    if (canvas._chart) canvas._chart.destroy();
    canvas._chart = new Chart(canvas, {
      type,
      data: {
        labels,
        datasets: [{ label: 'Score %', data: values, backgroundColor: accent, borderColor: accent, borderWidth: 2 }]
      },
      options: {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: type === 'radar' ? {} : { y: { min: 0, max: 100 } }
      }
    });
  }
};
