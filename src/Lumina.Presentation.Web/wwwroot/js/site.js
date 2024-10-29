const scrollbarHeight = getScrollbarHeight();
const scrollbarWidth = getScrollbarWidth();
const progressIndicator = document.getElementById('progress-indicator');
const progressIndicatorValue = document.getElementById('progress-indicator-value');
const progressIndicatorValueShadow = document.getElementById('progress-indicator-value-shadow');
const progressIndicatorValueText = document.getElementById('progress-indicator-value-text');
const audioPlayerFullHeightToggle = document.getElementById('audio-player-full-height-toggle');
const audioPlayerClose = document.getElementById('audio-player-close');

let enableConsoleDebugMessages = false; // whether to display debug messages at the developer console, or not

var ajaxCallCounter = 0;


//+======================================================================================+
//|                                 API interaction                                      |
//+======================================================================================+ 

/**
 * Performs a GET request to the specified API endpoint.
 * @param {string} url - The API endpoint URL.
 * @returns {Promise<any>} The response data if successful, undefined otherwise.
 */
async function callApiGetAsync(url) {
    try {
        // create the URL for the API endpoint
        const response = await fetch(url); // call the API
        if (!response.ok)
            throw new Error('Network response was not ok');
        const jsonResponse = await response.json();
        if (jsonResponse.success)
            return jsonResponse.data;
        else
            notificationService.show(jsonResponse.errorMessage, NotificationType.ERROR);
    } catch (error) {
        console.error('Error:', error);
        notificationService.show(error, NotificationType.ERROR);
    }
}

//+======================================================================================+
//|                                Website navigation                                    |
//+======================================================================================+

/**
 * Initializes navigation by setting the initial history state, intercepting link clicks,
 * and managing back/forward browser actions.
 */
function initializeNavigation() {
    // store initial state
    const initialState = {
        url: window.location.href,
        content: document.querySelector('main').innerHTML,
        title: document.title
    };
    history.replaceState(initialState, document.title, window.location.href);
    // initial nav-link binding
    bindNavigationLinks();

    // handle browser back/forward
    window.addEventListener('popstate', handlePopState);
}


/**
 * Binds AJAX navigation behavior to links marked with .nav-link class.
 */
function bindNavigationLinks() {
    document.querySelectorAll('.nav-link').forEach(link => {
        // only bind if not already bound
        if (!link.dataset.boundNavigation) {
            link.addEventListener('click', handleNavigation);
            link.dataset.boundNavigation = 'true';
        }
    });
}

/**
 * Handles link clicks to navigate dynamically without reloading the page.
 * @param {Event} e - The click event for a navigation link.
 */
async function handleNavigation(e) {
    e.preventDefault();
    const url = e.target.href;
    await updateContent(url, true);
}

/**
 * Restores page content and title when navigating with browser back/forward buttons.
 * @param {PopStateEvent} e - The popstate event with previous state data.
 */
async function handlePopState(e) {
    if (e.state) {
        // if we have cached content in state, use it immediately
        if (e.state.content) {
            document.querySelector('main').innerHTML = e.state.content;
            document.title = e.state.title;
        }
        // still fetch fresh content to ensure it's up to date
        await updateContent(e.state.url, false);
    }
}

/**
 * Loads new page content and updates the URL and history if required.
 * @param {string} url - URL to fetch new content from.
 * @param {boolean} addToHistory - Whether to add the new state to history.
 */
async function updateContent(url, addToHistory) {
    try {
        const response = await fetch(url);
        const html = await response.text();

        // parse the new content
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const content = doc.querySelector('main').innerHTML;
        const title = doc.querySelector('title')?.textContent || document.title;

        // update the page
        document.querySelector('main').innerHTML = content;
        document.title = title;

        // bind navigation links in the new content
        bindNavigationLinks();

        // save to history if needed
        if (addToHistory) {
            const state = {
                url: url,
                content: content,
                title: title
            };
            history.pushState(state, title, url);
        }
    } catch (error) {
        console.error('Navigation failed:', error);
    }
}

//+======================================================================================+
//|                             Common Helper Functions                                  |
//+======================================================================================+ 

/**
 * Starts progress indicator
 */
function showBusyIndicator() {
    //if (ajaxCallCounter === 0)
        //progressIndicator.classList.remove('hidden');
    ajaxCallCounter++;
}

/**
 * Hides progress indicator
 */
function hideBusyIndicator() {
    ajaxCallCounter--;
    //if (ajaxCallCounter === 0)
        //progressIndicator.classList.add('hidden');
}

/**
 * Shows the elements that display a progress operation
 */
function showOperationProgress() {
    progressIndicatorValueShadow.classList.remove('hidden');
    progressIndicatorValue.classList.remove('hidden');
}

/**
 * Hides the elements that display a progress operation
 */
function hideOperationProgress() {
    progressIndicatorValueShadow.classList.add('hidden');
    progressIndicatorValue.classList.add('hidden');
}

/**
 * Updates the progress indicator
 * @param {int} completed - Number of completed elements
 * @param {int} total - Number of total elements
 */
function updateOperationProgress(completed, total) {
    progressIndicatorValueText.innerText = completed + '/' + total;
}

/**
 * Convert Blob to Base64.
 * @param {any} blob The blob to convert.
 * @returns The converted Base64 string.
 */
async function blobToBase64(blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result.split(',')[1]);
        reader.onerror = reject;
        reader.readAsDataURL(blob);
    });
}

/**
 * Determines if the given file path corresponds to an image file.
 * @param {string} filePath - The path to the file to be checked.
 * @returns {boolean} True if the file path ends with an image file extension, false otherwise.
 */
function isImageFile(filePath) {
    // Adjust the regex pattern to match the image file extensions you support
    return /\.(jpe?g|png|gif|bmp|svg|ico|webp)$/i.test(filePath);
}

/**
 * Gets the current time
 * @returns the current time
 */
function getCurrentTime() {
    const now = new Date();
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');
    const milliseconds = String(now.getMilliseconds()).padStart(3, '0');
    return `${hours}:${minutes}:${seconds}:${milliseconds}`;
}

/**
 * Determines if the given element has vertical and/or horizontal scrollbars.
 * @param {HTMLElement} element - The element to check for scrollbars.
 * @returns {Object} An object indicating the presence of vertical and horizontal scrollbars.
 * @property {boolean} vertical - True if the element has a vertical scrollbar, otherwise false.
 * @property {boolean} horizontal - True if the element has a horizontal scrollbar, otherwise false.
 */
function hasScrollbars(element) {
    const tolerance = 3;
    return {
        vertical: element.scrollHeight - element.clientHeight > tolerance,
        horizontal: element.scrollWidth - element.clientWidth > tolerance
    };
}

/**
 * Get the height of the browser's scrollbar.
 * @returns {number} Height of the scrollbar in pixels.
 */
function getScrollbarHeight() {
    // create an outer div and set its height and overflowY properties
    const outer = document.createElement('div');
    outer.style.visibility = 'hidden';
    outer.style.height = '100px';
    outer.style.overflowY = 'scroll';
    document.body.appendChild(outer);
    const heightNoScroll = outer.offsetHeight;
    const heightWithScroll = outer.clientHeight;
    // cleanup after measuring
    outer.parentNode.removeChild(outer);
    return heightNoScroll - heightWithScroll;
}

/**
 * Get the width of the browser's scrollbar.
 * @returns {number} Width of the scrollbar in pixels.
 */
function getScrollbarWidth() {
    // create an outer div and set its width
    const outer = document.createElement('div');
    outer.style.visibility = 'hidden';
    outer.style.width = '100px';
    document.body.appendChild(outer);
    const widthNoScroll = outer.offsetWidth;
    // apply overflow and append an inner div
    outer.style.overflow = 'scroll';
    const inner = document.createElement('div');
    inner.style.width = '100%';
    outer.appendChild(inner);
    const widthWithScroll = inner.offsetWidth;
    // cleanup and calculate the difference
    outer.parentNode.removeChild(outer);
    return widthNoScroll - widthWithScroll;
}

/**
 * Handles the wheel event for horizontal scrolling.
 * @param {WheelEvent} event - The wheel event object.
 */
const scrollHandler = function (event) {
    event.preventDefault();
    this.scrollLeft += (event.deltaY > 0 ? 1 : -1) * 80;
};

/**
 * Enables horizontal scrolling to the element identified by the specified id or element.
 * @param {string|HTMLElement} idOrElement - The id of the element or the element itself.
 */
function addHorizontalScrolling(idOrElement) {
    let element = idOrElement;
    if (typeof idOrElement === 'string') 
        element = document.getElementById(idOrElement);
    if (element instanceof HTMLElement) 
        element.addEventListener('wheel', scrollHandler, { passive: false });
    else 
        console.error('Invalid input: Expected an element ID or HTMLElement');
}

/**
 * Disables horizontal scrolling for the element identified by the specified id or element.
 * @param {string|HTMLElement} idOrElement - The id of the element or the element itself.
 */
function removeHorizontalScrolling(idOrElement) {
    let element = idOrElement;
    if (typeof idOrElement === 'string') 
        element = document.getElementById(idOrElement);
    if (element instanceof HTMLElement) 
        element.removeEventListener('wheel', scrollHandler, { passive: false });
    else 
        console.error('Invalid input: Expected an element ID or HTMLElement');
}

/**
 * Gets the position and size of an element relative to the viewport.
 * @param {string} id - The Id of the element to get the offset for.
 * @returns {Object} An object with the top, left, width, and height of the element.
 */
function getElementOffset(id) {
    const element = document.getElementById(id);
    if (element) {
        const rect = element.getBoundingClientRect();
        return {
            top: rect.top + window.scrollY,
            left: rect.left + window.scrollX,
            width: rect.width,
            height: rect.height
        };
    }
    return {
        top: 0,
        left: 0,
        width: 0,
        height: 0
    };
}

/**
 * Toggles the visibility of the audio player.
 */
function toggleAudioPlayerVisibility() {
    const playerContainer = document.querySelector('.audio-player-container');
    const contentContainer = document.querySelector('main');
    playerContainer.classList.toggle('not-shown');
    contentContainer.classList.toggle('audio-player-not-shown');
}

/**
 * Toggles the display of the audio player as collapsed player, or full height player.
 */
function toggleAudioPlayerFullHeight() {
    const playerContainer = document.querySelector('.audio-player-container');
    const contentContainer = document.querySelector('main');
    playerContainer.classList.toggle('full-height');
    contentContainer.classList.toggle('audio-player-full-height');
}

audioPlayerFullHeightToggle.addEventListener('click', toggleAudioPlayerFullHeight);
audioPlayerClose.addEventListener('click', toggleAudioPlayerVisibility);

document.addEventListener('DOMContentLoaded', initializeNavigation);
