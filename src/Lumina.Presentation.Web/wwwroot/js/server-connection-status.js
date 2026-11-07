/**
 * Shows the global server unreachable banner while the health probe or a SignalR connection reports the server as
 * unreachable. The banner markup comes from the theme template, with a fallback created here for themes that do not
 * provide one.
 */
(function () {
    'use strict';

    const unreachableSources = new Set();
    let banner = null;

    /**
     * Creates the banner the first time it is needed, and appends it to the body, so it survives the AJAX navigation
     * that only swaps the content of the main element.
     * @returns {HTMLElement} The banner element.
     */
    function getBanner() {
        if (banner !== null && banner.isConnected)
            return banner;
        // A theme renders the banner through its shared/server-unreachable-banner template, and the application renders a fallback
        // when it does not. The data hook is the contract the script toggles.
        banner = document.querySelector('[data-server-unreachable-banner]');
        if (banner !== null)
            return banner;
        banner = document.createElement('div');
        banner.id = 'server-unreachable-banner';
        banner.className = 'server-unreachable-banner';
        banner.setAttribute('data-server-unreachable-banner', '');
        banner.setAttribute('role', 'status');
        banner.setAttribute('aria-live', 'polite');
        banner.setAttribute('aria-atomic', 'true');
        banner.textContent = window.jsLocalizedMessages?.serverUnreachable ?? 'Server is unreachable';
        document.body.appendChild(banner);
        return banner;
    }

    /**
     * Shows the banner while at least one live connection is unreachable, and hides it once every connection recovered.
     */
    function refreshBanner() {
        getBanner().classList.toggle('is-visible', unreachableSources.size > 0);
    }

    window.serverConnectionStatus = {
        /**
         * Marks a live connection as unreachable, showing the global banner.
         * @param {string} source - The name of the connection that lost its connection.
         */
        markUnreachable: function (source) {
            unreachableSources.add(source);
            refreshBanner();
        },

        /**
         * Marks a live connection as reachable again, hiding the global banner when no other connection is unreachable.
         * @param {string} source - The name of the connection that reconnected.
         */
        markReachable: function (source) {
            unreachableSources.delete(source);
            refreshBanner();
        }
    };

    // The health endpoint is the page independent source of the banner: it reports the API as unreachable even on pages
    // that have no SignalR connection, so the banner appears on every page while the server cannot serve requests.
    const HEALTH_CHECK_INTERVAL_MS = 5000;
    const HEALTH_CHECK_TIMEOUT_MS = 4000;
    const HEALTH_CHECK_SOURCE = 'healthCheck';
    let isHealthCheckInFlight = false;

    /**
     * Schedules the next health probe, with a little jitter so many open tabs do not probe in lockstep.
     */
    function scheduleNextHealthCheck() {
        const jitter = Math.round((Math.random() * 2 - 1) * HEALTH_CHECK_INTERVAL_MS * 0.2);
        setTimeout(() => {
            if (document.visibilityState === 'visible')
                checkServerHealthAsync();
            else
                scheduleNextHealthCheck();
        }, Math.max(1000, HEALTH_CHECK_INTERVAL_MS + jitter));
    }

    /**
     * Checks whether the server can reach the API and updates the global banner accordingly.
     */
    async function checkServerHealthAsync() {
        const healthCheckUrl = window.globalConfiguration?.healthCheckUrl;
        if (!healthCheckUrl || isHealthCheckInFlight) {
            scheduleNextHealthCheck();
            return;
        }
        isHealthCheckInFlight = true;
        const controller = new AbortController();
        const timeoutHandle = setTimeout(() => controller.abort(), HEALTH_CHECK_TIMEOUT_MS);
        try {
            const response = await fetch(healthCheckUrl, {
                headers: { 'Accept': 'application/json' },
                cache: 'no-store',
                signal: controller.signal
            });
            if (response.ok) {
                // The API is reachable. Only the sources that describe server reachability are cleared; a disconnected
                // SignalR hub keeps its own source until it reconnects.
                unreachableSources.delete(HEALTH_CHECK_SOURCE);
                unreachableSources.delete('api');
                window.isServerUnreachable = false;
                refreshBanner();
            } else {
                unreachableSources.add(HEALTH_CHECK_SOURCE);
                refreshBanner();
            }
        } catch {
            unreachableSources.add(HEALTH_CHECK_SOURCE);
            refreshBanner();
        } finally {
            clearTimeout(timeoutHandle);
            isHealthCheckInFlight = false;
            scheduleNextHealthCheck();
        }
    }

    checkServerHealthAsync();

    refreshBanner();
})();
