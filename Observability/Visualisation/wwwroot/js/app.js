Chart.register(ChartZoom);

class TraceVisualizer {
    constructor() {
        this.traces = [];
        this.filteredTraces = [];
        this.selectedTrace = null;

        // DOM элементы
        this.treeContainer = document.getElementById('requestTree');
        this.timelineContainer = document.getElementById('timeline');
        this.detailsContainer = document.getElementById('requestDetails');

        // Инициализация
        this.initializeEventListeners();
        this.loadTraces();
        this.connectWebSocket();

        this.timelineWindowStart = null; // начало видимой области
        this.timelineBucketSize = 15000;
        this.timelineBucketCount = 20;
        this._userScrolled = false;
    }

    initializeEventListeners() {
        document.querySelectorAll('.accordion-item h3').forEach(header => {
            header.addEventListener('click', () => {
                const content = header.nextElementSibling;
                content.classList.toggle('active');
            });
        });
    }

    async loadTraces() {
        try {
            const response = await fetch('/api/traces/debug');
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            this.traces = await response.json();
            this.filteredTraces = this.traces; // теперь всегда все трейсы
            this.renderTree();
            this.renderTimeline();
        } catch (error) {
            console.error('Ошибка загрузки трейсов:', error);
            this.showError('Не удалось загрузить трейсы');
        }
    }

    

    showError(message) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'error-message';
        errorDiv.textContent = message;
        document.body.appendChild(errorDiv);
        setTimeout(() => {
            errorDiv.remove();
        }, 3000);
    }

    renderTree() {
        this.treeContainer.innerHTML = '';
        const allTraceIds = new Set(this.filteredTraces.map(t => t.TraceId));
        const rootTraces = this.filteredTraces.filter(
            t => t.ParentId === '0' || !allTraceIds.has(t.ParentId)
        );
        rootTraces.forEach(rootTrace => {
            this.renderTreeNode(rootTrace, this.filteredTraces, 0);
        });
    }

    renderTreeNode(trace, allTraces, level) {
        const node = document.createElement('div');
        node.className = 'tree-node';
        node.style.marginLeft = `${level * 24}px`;
        const host = trace.HttpRequestData?.['header:Host']?.toString() || '';
        if (host.includes('balancer')) {
            node.classList.add('balancer');
        } else if (host.includes('localhost')) {
            node.classList.add('service');
        }
        if (trace.IsError) {
            node.classList.add('error');
        }
        node.innerHTML = `
            <div class="node-header">
                <span class="node-icon">${this.getNodeIcon(trace)}</span>
                <span class="node-id">${trace.NodeId}</span>
            </div>
            <div class="node-details">
                <div>Method: ${trace.HttpRequestData?.method || 'N/A'}</div>
                <div>Path: ${trace.HttpRequestData?.path || 'N/A'}</div>
                <div>Duration: ${trace.MetricData?.duration || 'N/A'}ms</div>
            </div>
        `;
        node.addEventListener('click', (e) => {
            e.stopPropagation();
            this.selectTrace(trace);
        });
        this.treeContainer.appendChild(node);
        const childTraces = allTraces.filter(t => t.ParentId === trace.TraceId);
        childTraces.forEach(childTrace => {
            this.renderTreeNode(childTrace, allTraces, level + 1);
        });
    }

    getNodeIcon(trace) {
        const nodeId = trace.NodeId || '';
        if (nodeId.startsWith('balance')) return '🌐';
        if (nodeId.startsWith('fir') || nodeId.startsWith('sec')) return '⚙️';
        return '🔷';
    }

    renderTimeline() {
        const canvas = this.timelineContainer;
        const ctx = canvas.getContext('2d');

        canvas.width = this.timelineContainer.offsetWidth;
        canvas.height = 140;

        if (this.filteredTraces.length === 0) return;

        const traces = this.filteredTraces;
        const classify = nodeId => nodeId?.split('-')[0] || 'unknown';

        const bucketSize = this.timelineBucketSize;
        const classifyTypes = [...new Set(traces.map(t => classify(t.NodeId)))];
        const typeColors = {
            balance: '#2196f3',
            fir: '#4caf50',
            sec: '#ff9800',
            unknown: '#9e9e9e'
        };

        // Построим мапу { bucket: { type: count } }
        const buckets = new Map();
        let minTime = Infinity, maxTime = -Infinity;

        for (const trace of traces) {
            const t = new Date(trace.Timestamp).getTime();
            const bucket = Math.floor(t / bucketSize) * bucketSize;
            const type = classify(trace.NodeId);
            if (!buckets.has(bucket)) buckets.set(bucket, {});
            const b = buckets.get(bucket);
            b[type] = (b[type] || 0) + 1;

            minTime = Math.min(minTime, bucket);
            maxTime = Math.max(maxTime, bucket);
        }

        // Обновим "окно"
        const now = maxTime;
        if (!this._userScrolled) {
            this.timelineWindowStart = now - bucketSize * this.timelineBucketCount;
        }

        const labels = [];
        const dataByType = {};
        classifyTypes.forEach(t => dataByType[t] = []);

        for (let t = this.timelineWindowStart; t <= now; t += bucketSize) {
            labels.push(new Date(t).toLocaleTimeString());
            const b = buckets.get(t) || {};
            classifyTypes.forEach(type => {
                dataByType[type].push(b[type] || 0);
            });
        }

        if (this.timelineChart) this.timelineChart.destroy();
        
        this.timelineChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels,
                datasets: classifyTypes.map(type => ({
                    label: type,
                    data: dataByType[type],
                    borderColor: typeColors[type] || '#000',
                    backgroundColor: 'transparent',
                    borderWidth: 2,
                    tension: 0.3
                }))
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false,
                plugins: {
                    legend: { position: 'top' },
                    tooltip: {
                        callbacks: {
                            label: ctx => `${ctx.dataset.label}: ${ctx.parsed.y}`
                        }
                    },
                    zoom: {
                        pan: {
                            enabled: true,
                            mode: 'x',
                            modifierKey: null,  // убрать shift/ctrl, чтобы можно было просто тянуть
                            onPan: ({ chart }) => {
                                this._userScrolled = true;
                            },
                            scaleMode: 'x'
                        },
                        zoom: {
                            wheel: { enabled: true },
                            pinch: { enabled: true },
                            mode: 'x'
                        }
                    }
                },
                scales: {
                    x: {
                        title: { display: true, text: 'Время' },
                        ticks: {
                            maxRotation: 0
                        }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { precision: 0 }
                    }
                }
            }
        });
    }



    selectTrace(trace) {
        this.selectedTrace = trace;
        this.renderDetails();
    }

    renderDetails() {
        const httpData = document.getElementById('httpRequestData');
        const metricsChart = document.getElementById('metricsChart');
        if (!this.selectedTrace) {
            httpData.innerHTML = '';
            metricsChart.innerHTML = '';
            return;
        }

        // Детали запроса (таблица)
        const requestData = this.selectedTrace.HttpRequestData || {};
        const formattedRequestData = `
        <div class="request-details">
            <h3>Детали запроса</h3>
            <table class="request-table">
                <tr>
                    <td>Метод:</td>
                    <td>${requestData.method || 'N/A'}</td>
                </tr>
                <tr>
                    <td>Путь:</td>
                    <td>${requestData.path || 'N/A'}</td>
                </tr>
                <tr>
                    <td>Хост:</td>
                    <td>${requestData['header:Host'] || 'N/A'}</td>
                </tr>
                <tr>
                    <td>Trace ID:</td>
                    <td>${this.selectedTrace.TraceId || 'N/A'}</td>
                </tr>
                <tr>
                    <td>Parent ID:</td>
                    <td>${this.selectedTrace.ParentId || 'N/A'}</td>
                </tr>
                <tr>
                    <td>Node ID:</td>
                    <td>${this.selectedTrace.NodeId || 'N/A'}</td>
                </tr>
            </table>
        </div>
    `;
        httpData.innerHTML = formattedRequestData;

        // Метрики (плашки)
        if (this.selectedTrace.MetricData) {
            const duration = (this.selectedTrace.MetricData.duration || 0) / 1000;
            const memory = (this.selectedTrace.MetricData.memory || 0) / 1024;
            const cpu = this.selectedTrace.MetricData.cpu || 0;

            metricsChart.innerHTML = `
            <div class="metrics-column">
                <div class="metric-card">
                    <h3>Длительность</h3>
                    <div class="metric-data">
                        <span class="metric-value">${duration.toFixed(2)}</span>
                        <span class="metric-unit">сек</span>
                    </div>
                </div>
                <div class="metric-card">
                    <h3>Память</h3>
                    <div class="metric-data">
                        <span class="metric-value">${memory.toFixed(2)}</span>
                        <span class="metric-unit">КБ</span>
                    </div>
                </div>
                <div class="metric-card">
                    <h3>CPU</h3>
                    <div class="metric-data">
                        <span class="metric-value">${cpu}</span>
                        <span class="metric-unit">%</span>
                    </div>
                </div>
            </div>
        `;
        } else {
            metricsChart.innerHTML = '';
        }
    }

    connectWebSocket() {
        const wsProtocol = location.protocol === 'https:' ? 'wss' : 'ws';
        // Порт 5050 - Storage, если другой, поменяйте на нужный
        const wsUrl = `${wsProtocol}://${location.hostname}:5050/ws/updates`;
        this.ws = new WebSocket(wsUrl);
        this.ws.onmessage = (event) => {
            try {
                console.log("[WS] Получено сообщение");
                const newTrace = JSON.parse(event.data);
                this.traces.push(newTrace);
                this.filteredTraces = this.traces;
                this.renderTree();
                this.renderTimeline(); // ⬅️ обязательно!
            } catch (err) {
                console.error('Ошибка парсинга WebSocket-сообщения:', err, event.data);
            }
        };
        this.ws.onclose = () => {
            setTimeout(() => this.connectWebSocket(), 2000);
        };
        this.ws.onopen = () => console.log('[WS] Подключено');
        this.ws.onclose = () => console.log('[WS] Отключено');
        this.ws.onerror = (e) => console.error('[WS] Ошибка', e);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new TraceVisualizer();
});