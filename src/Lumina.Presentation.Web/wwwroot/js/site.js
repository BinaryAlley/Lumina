const scrollbarHeight = getScrollbarHeight();
const scrollbarWidth = getScrollbarWidth();

let selectionStartPosition = { x: 0, y: 0 };
let currentMousePosition = { x: 0, y: 0 };
let isSelecting = false;
let isSelectionMode = false;

let fileSystemExplorer = null;
let fileSystemExplorerContainer = null;
let visibleSelectionRectangle = null;
let selectionRectangle = null;

// =================== common helper function ===================

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
 * Enables horizontal scrolling to the element identified by the specified id.
 */
function addHorizontalScrolling(id) {
    document.getElementById(id).addEventListener('wheel', function (e) {
        e.preventDefault();
        this.scrollLeft += (e.deltaY > 0 ? 1 : -1) * 80;
    }, { passive: false });
}

/**
 * Disables horizontal scrolling to the element identified by the specified id.
 */
function removeHorizontalScrolling(id) {
    document.getElementById(id).removeEventListener('wheel', function (e) {
        e.preventDefault();
        this.scrollLeft += (e.deltaY > 0 ? 1 : -1) * 80;
    }, { passive: false });
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
 * Get the position and size of an HTML element.
 * @param {string} id - The id of the element to get the rectangle of.
 * @returns {object} An object containing the element's X (left), Y (top), Width, and Height in pixels.
 */
function getElementRectangle(id) {
    const element = document.getElementById(id);
    const rect = element.getBoundingClientRect();
    return {
        x: rect.left,
        y: rect.top,
        width: rect.width,
        height: rect.height
    };
}

// =================== file system browser dialog ===================

/**
 * Adjusts the height of a dropdown identified by `comboboxId` to fit in the available space.
 * @param {string} comboboxId The Id of the combobox whose dropdown should be adjusted.
 */
function adjustDropdownHeight(comboboxId = null) {
    const comboboxes = comboboxId
        ? [document.getElementById(comboboxId)] // if comboboxId is provided, use only that element
        : document.querySelectorAll('[id^="combobox_"]:not([id^="combobox_checkbox_"])'); // otherwise, select all dropdowns
    if (comboboxes) {
        comboboxes.forEach(combobox => {
            if (!combobox) return; // skip if no element is found
            const dropdown = combobox.querySelector('.enlightenment-dropdown');
            if (!dropdown) return; // skip if there's no dropdown inside
            // get the bounds of the combobox and the height of the viewport
            const comboboxRect = combobox.getBoundingClientRect();
            const viewportHeight = window.innerHeight;
            // calculate the space available between the combobox and the bottom of the viewport, with a 10px margin
            const dropdownTopPosition = comboboxRect.top + comboboxRect.height;
            const availableSpace = viewportHeight - dropdownTopPosition - 10; // 10px margin
            // adjust the dropdown height
            dropdown.style.maxHeight = `${availableSpace}px`;
        });
    }
}

/**
 * Updates the position and height of the dropdown relative to the specified path segment.
 * @param {string} pathSegmentId - The ID of the path segment element to position the dropdown relative to.
 */
function updateDropdownPosition(pathSegmentId) {
    const element = document.getElementById(pathSegmentId);
    const dropdown = document.getElementById('navigatorDropdown');
    if (element && dropdown) {
        const rect = getElementOffset(pathSegmentId);
        dropdown.style.top = `${rect.top + rect.height}px`;
        dropdown.style.left = `${rect.left}px`;
        // calculate the available height between the dropdown and the bottom of the viewport
        const viewportHeight = window.innerHeight;
        const availableHeight = viewportHeight - (rect.top + rect.height) - 10; // 10 px margin
        // set the dropdown's max height, ensuring it doesn't exceed the available height
        dropdown.style.maxHeight = `${availableHeight}px`;
    }
}

/**
 * Focuses the navigator address bar input.
 */
function focusNavigatorInputElement() {
    const elements = document.getElementsByClassName('navigator-address-bar-input');
    if (elements.length > 0)
        elements[0].focus();
}

/**
 * Unchecks all checkboxes except the one with the specified Id.
 * @param {string} id - The Id of the checkbox to leave checked.
 */
function uncheckAllExcept(id) {
    const checkboxes = document.querySelectorAll('.navigator-toggle-checkbox');
    checkboxes.forEach((checkbox) => {
        if (checkbox.id !== id)
            checkbox.checked = false;
    });
}

/**
 * Closes all navigator path segments.
 */
function closeNavigatorPathSegments() {
    const checkboxes = document.getElementsByClassName('navigator-toggle-checkbox');
    for (let i = 0; i < checkboxes.length; i++)
        checkboxes[i].checked = false;
}

// =================== file system browser dialog selection ===================

/**
 * Function called at the initialization of fle system browser dialog
 * @param {any} dotnetHelper - .NET object to invoke methods asynchronously.
 */
function initializeFileSystemBrowser(dotnetHelper) {
    // cache frequently accessed elements
    if (!selectionRectangle)
        selectionRectangle = document.getElementById('selection-rectangle');
    if (!visibleSelectionRectangle)
        visibleSelectionRectangle = document.getElementById('visible-selection-rectangle');
    if (!fileSystemExplorer) {
        fileSystemExplorer = document.getElementById('file-system-browser-file-system-explorer');
        if (fileSystemExplorer) 
            fileSystemExplorer.addEventListener('mouseup', fileSystemExplorerMouseUp);
    }
    // event handlers needed for selection
    if (!fileSystemExplorerContainer) {
        fileSystemExplorerContainer = document.getElementById('file-system-browser-file-system-explorer-container');
        if (fileSystemExplorerContainer) {
            fileSystemExplorerContainer.addEventListener('mousedown', startSelection);
            fileSystemExplorerContainer.addEventListener('mousemove', updateSelection);
            fileSystemExplorerContainer.addEventListener('mouseup', endSelection);
            fileSystemExplorerContainer.addEventListener('wheel', handleSelection);
            fileSystemExplorerContainer.addEventListener('scroll', handleSelection);
        }
    }
    // handler for the mouse down event of file system browser dialog files and directories elements
    window.handleFileSystemEntityMouseDown = function (e) {
        // stop the event from bubbling up to the explorer - we dont want to start selection rectangles when clicking files or directories
        e.stopPropagation();
    };

    // handler for the mouse up event of file system browser dialog files and directories elements
    window.handleFileSystemEntityMouseUp = function (e) {
        e.preventDefault();
        if (!isSelecting)
            e.stopPropagation();
        const allEClassElements = Array.from(fileSystemExplorer.querySelectorAll('.e'));
        const currentIndex = allEClassElements.indexOf(e.currentTarget);

        // If a previous click timestamp exists, check the time difference
        const previousTime = parseInt(e.currentTarget.dataset.selectionStartTime || 0);
        const currentTime = new Date().getTime();
        const timeDifference = currentTime - previousTime;

        // TODO: needs to account for when the element is already selected, and a double click occurs - on first click it would check the difference and go to rename -
        // maybe just wait half second before performing action

        // if time difference is less than 300ms, treat it as a double-click
        if (timeDifference < 300) {
            if (e.currentTarget.dataset.type === 'directory') { // directories
                dotnetHelper.invokeMethodAsync('OnDirectoryDoubleClickAsync', e.currentTarget.dataset.path);
                return;
            }
            else
            { // files

            }
            // reset the timestamp used to track double clicks
            e.currentTarget.dataset.selectionStartTime = 0;
            return;
        }
        else
            e.currentTarget.dataset.selectionStartTime = currentTime; // element was single clicked, update the selection start time to now 
        if (e.ctrlKey || isSelectionMode) { // when CTRL key is pressed
            // toggle selection for the current clicked item
            e.currentTarget.classList.toggle('selected');
            // set the selection start if it's not already set
            if (fileSystemExplorer.dataset.selectionStart === undefined)
                fileSystemExplorer.dataset.selectionStart = currentIndex;
        }
        else if ((e.shiftKey && fileSystemExplorer.dataset.selectionStart !== undefined) || isSelectionMode) { // when SHIFT key is pressed
            const start = parseInt(fileSystemExplorer.dataset.selectionStart);
            // remove all selections
            allEClassElements.forEach(elem => {
                elem.classList.remove('selected');
            });
            // define the range to select items from and to
            const [from, to] = start < currentIndex ? [start, currentIndex] : [currentIndex, start];
            // add the 'selected' class to items in the range
            for (let i = from; i <= to; i++)
                allEClassElements[i].classList.add('selected');
        }
        else
        { // when neither CTRL nor SHIFT keys are pressed
            if (!isSelectionMode) {
                // get all elements that are selected
                const selectedElements = allEClassElements.filter(element => element.classList.contains('selected'));
                // add 'selected' class to the current clicked item
                e.currentTarget.classList.add('selected');
                // if there are no selected elements, or if there are more than one, deselect all of them, and select just the clicked one
                if ((selectedElements.length === 1 && e.currentTarget !== selectedElements[0]) || selectedElements.length !== 1) {
                    // deselect all items
                    selectedElements.forEach(elem => {
                        if (elem !== e.currentTarget) {
                            elem.classList.remove('selected');
                            // remove selectionStartTime from all items
                            delete elem.dataset.selectionStartTime;
                        }
                    });
                    // set the current item as the selection start
                    fileSystemExplorer.dataset.selectionStart = currentIndex;
                    // store the current timestamp for future double-click checks
                    e.currentTarget.dataset.selectionStartTime = currentTime;
                }
            }
            else // if its selection mode, just toggle selection of item, when clicked
                e.currentTarget.classList.toggle('selected');
        }
    };
}

/**
 * Handler for the mouse up event of the file system browser dialog explorer
 * @param {MouseEvent} e - The mouseup event object.
 */
function fileSystemExplorerMouseUp(e) {
    const explorerScrollbars = hasScrollbars(fileSystemExplorer);
    // check if clicked on the horizontal scrollbar
    if (explorerScrollbars.horizontal && e.clientY > fileSystemExplorer.getBoundingClientRect().top + fileSystemExplorer.offsetHeight - scrollbarHeight) 
        return;
    if (!e.ctrlKey && !e.shiftKey && !isSelectionMode) {
        // deselect all file system elements when their explorer is clicked
        fileSystemExplorer.querySelectorAll('.e.selected').forEach(item => {
            item.classList.remove('selected');
        });
        // do not store information about what item was clicked first, in context of Shift-selection
        fileSystemExplorer.removeAttribute('data-selection-start');
    }
}

/**
 * Handler for the file system browser dialog selection start.
 * @param {MouseEvent} e - The mousedown event object.
 */
function startSelection(e) {
    const explorerScrollbars = hasScrollbars(fileSystemExplorer);
    const containerExplorerScrollbars = hasScrollbars(fileSystemExplorerContainer);
    // check if clicked on the horizontal or vertical scrollbar
    if (explorerScrollbars.horizontal && e.clientY > fileSystemExplorer.getBoundingClientRect().top + fileSystemExplorer.offsetHeight - scrollbarHeight)
        return;
    if (containerExplorerScrollbars.vertical && e.clientX > fileSystemExplorerContainer.getBoundingClientRect().left + fileSystemExplorerContainer.offsetWidth - scrollbarWidth)
        return;
    // store information about selection start
    isSelecting = true;
    document.body.style.overflowY = 'hidden'; // needed in order to not put a vertical scrollbar on the document, while in vertical layout selection
    document.documentElement.style.overflowY = 'hidden';
    selectionStartPosition.x = e.pageX + fileSystemExplorer.scrollLeft;
    selectionStartPosition.y = e.pageY + fileSystemExplorerContainer.scrollTop; // not a mistake - in vertical mode, this is the overflow container!
    // place the selection rectangle at the clicked location (relative to the whole page, and taking into account scroll offset), with 0 size
    visibleSelectionRectangle.style.left = e.pageX + 'px';
    visibleSelectionRectangle.style.top = e.pageY + 'px';
    visibleSelectionRectangle.style.width = '0px';
    visibleSelectionRectangle.style.height = '0px';
    visibleSelectionRectangle.style.visibility = 'visible';

    selectionRectangle.style.left = e.pageX + 'px';
    selectionRectangle.style.top = e.pageY + 'px';
    selectionRectangle.style.width = '0px';
    selectionRectangle.style.height = '0px';
}

/**
 * Handler for the file system browser dialog selection update.
 * @param {MouseEvent} e - The mousemove event object.
 */
function updateSelection(e) {
    if (!isSelecting)
        return;
    currentMousePosition.x = e.pageX;
    currentMousePosition.y = e.pageY;
    handleSelection();
}

/**
 * Handler for the file system browser dialog selection end.
 * @param {MouseEvent} e - The mouseup event object.
 */
function endSelection(e) {
    // selection ended, hide the selection rectangle
    isSelecting = false;
    document.body.style.removeProperty('overflow-y');
    document.documentElement.style.removeProperty('overflow-y');

    visibleSelectionRectangle.style.visibility = 'hidden';
    selectionRectangle.style.visibility = 'hidden';

    selectionRectangle.style.left = '0px';
    selectionRectangle.style.top = '0px';
    selectionRectangle.style.width = '0px';
    selectionRectangle.style.height = '0px';
    visibleSelectionRectangle.style.left = '0px';
    visibleSelectionRectangle.style.top = '0px';
    visibleSelectionRectangle.style.width = '0px';
    visibleSelectionRectangle.style.height = '0px';
    // go through each .e item that was inside the selection rectangle, and mark it as selected
    fileSystemExplorer.querySelectorAll('.e.selection-hover').forEach(item => {
        item.classList.add('selected');
        item.classList.remove('selection-hover');
    });
}

/**
 * Function for handling the file system browser dialog selection.
 */
function handleSelection() {
    if (!isSelecting) // only relevant when we have a selection initialized
        return;
    // calculate dimensions of the selection rectangle
    const currentX = currentMousePosition.x;
    const currentY = currentMousePosition.y;
    const width = currentX - (selectionStartPosition.x - fileSystemExplorer.scrollLeft);
    const height = currentY - (selectionStartPosition.y - fileSystemExplorerContainer.scrollTop);

    // update selection rectangle dimensions
    visibleSelectionRectangle.style.width = Math.abs(width) + 'px';
    visibleSelectionRectangle.style.height = Math.abs(height) + 'px';
    selectionRectangle.style.width = Math.abs(width) + 'px';
    selectionRectangle.style.height = Math.abs(height) + 'px';

    // adjust position based on selection direction
    if (width < 0) {
        selectionRectangle.style.left = currentX + 'px';
        visibleSelectionRectangle.style.left = currentX + 'px';
    } else {
        selectionRectangle.style.left = (selectionStartPosition.x - fileSystemExplorer.scrollLeft) + 'px';
        visibleSelectionRectangle.style.left = (selectionStartPosition.x - fileSystemExplorer.scrollLeft) + 'px';
    }
    if (height < 0) {
        selectionRectangle.style.top = currentY + 'px';
        visibleSelectionRectangle.style.top = currentY + 'px';
    } else {
        selectionRectangle.style.top = (selectionStartPosition.y - fileSystemExplorerContainer.scrollTop) + 'px';
        visibleSelectionRectangle.style.top = (selectionStartPosition.y - fileSystemExplorerContainer.scrollTop) + 'px';
    }

    // do not let the selection rectangle go outside of explorer container, clip it if needed
    const containerRect = fileSystemExplorerContainer.getBoundingClientRect();
    const left = parseFloat(visibleSelectionRectangle.style.left);
    const top = parseFloat(visibleSelectionRectangle.style.top) + 1;
    const right = left + parseFloat(visibleSelectionRectangle.style.width);
    const bottom = top + parseFloat(visibleSelectionRectangle.style.height);

    if (left < containerRect.left) {
        const difference = containerRect.left - left;
        visibleSelectionRectangle.style.width = (parseFloat(visibleSelectionRectangle.style.width) - difference) + 'px';
        visibleSelectionRectangle.style.left = containerRect.left + 'px';
    }
    if (right > containerRect.right)
        visibleSelectionRectangle.style.width = (containerRect.right - left) + 'px';

    if (top < containerRect.top) {
        const difference = containerRect.top - top;
        visibleSelectionRectangle.style.height = (parseFloat(visibleSelectionRectangle.style.height) - difference) + 'px';
        visibleSelectionRectangle.style.top = containerRect.top + 'px';
    }
    if (bottom > containerRect.bottom)
        visibleSelectionRectangle.style.height = (containerRect.bottom - top) + 'px';

    // change color of items that fall within selection rectangle            
    const selectionRect = selectionRectangle.getBoundingClientRect();
    // go through each .e item which has not had its thumbnail retrieved yet
    fileSystemExplorer.querySelectorAll('.e').forEach(item => {
        const itemRect = item.getBoundingClientRect();
        // check if the item is fully or partially visible horizontally and vertically
        const isFullyVisibleHorizontally = itemRect.left >= selectionRect.left && itemRect.right <= selectionRect.right;
        const isFullyVisibleVertically = itemRect.top >= selectionRect.top && itemRect.bottom <= selectionRect.bottom;
        const isPartiallyVisibleHorizontally = itemRect.left < selectionRect.right && itemRect.right > selectionRect.left;
        const isPartiallyVisibleVertically = itemRect.top < selectionRect.bottom && itemRect.bottom > selectionRect.top;
        // if the item is fully or partially visible in both directions, add it to our list
        if ((isFullyVisibleHorizontally || isPartiallyVisibleHorizontally) && (isFullyVisibleVertically || isPartiallyVisibleVertically))
            item.classList.add('selection-hover');
        else
            item.classList.remove('selection-hover');
    });
}

/**
 * Toggles the state of selection mode.
 * @param {MouseEvent} e - The mouseup event object.
 */
function toggleSelectionMode(e) {
    isSelectionMode = !isSelectionMode;
}

// =================== window functions ===================

/**
 * Attaches a click event listener to the window, invoking a .NET method when the window is clicked.
 * @param {any} dotnetHelper - .NET object to invoke methods asynchronously.
 */
function windowClickHandler(dotnetHelper) {
    window.addEventListener('click', (e) => {
        dotnetHelper.invokeMethodAsync('OnWindowClickAsync', {
            x: e.clientX,
            y: e.clientY,
            sender: e.target.getAttribute('id')
        });
    });
}

/**
 * Attaches a keydown event listener to the given element, invoking a .NET method when 'Escape' is pressed.
 * @param {HTMLElement} element - The HTML element to attach the keydown listener to.
 * @param {any} dotnetHelper - .NET object to invoke methods asynchronously.
 */
function windowKeyDownHandler(element, dotnetHelper) {
    element.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            e.preventDefault();
            dotnetHelper.invokeMethodAsync('EscapePressed');
        }
    });
}

/**
 * Handles the window resize event.
 */
window.addEventListener('resize', function () {
    adjustDropdownHeight();
    // update all dropdowns' positions on resize
    const dropdown = document.getElementById('navigatorDropdown');
    if (dropdown) {
        const pathSegmentId = dropdown.getAttribute('data-path-segment-id');
        if (pathSegmentId)
            updateDropdownPosition(pathSegmentId);
    }
});