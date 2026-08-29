// A small, non-executable Mustache-style renderer, mirroring the subset of the server side ThemeTemplateEngine that the
// file system browser uses. The theme sub templates rendered by this script can place values and test truthiness, but cannot execute JavaScript.
// It is loaded lazily together with the file system browser dialog script, and exposed globally so that components can share it.
(function () {
    'use strict';

    /**
     * Escapes a value for safe inclusion in HTML text.
     * @param {any} value - The value to escape.
     * @returns {string} The HTML escaped text.
     */
    function escapeHtml(value) {
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#x27;');
    }

    /**
     * Determines whether a value is truthy, matching the truthiness rules of the server side engine: null, undefined, false, empty strings and empty arrays are falsy.
     * @param {any} value - The value to evaluate.
     * @returns {boolean} True when the value is truthy, false otherwise.
     */
    function isTruthy(value) {
        if (value === null || value === undefined || value === false)
            return false;
        if (typeof value === 'string')
            return value.length > 0;
        if (Array.isArray(value))
            return value.length > 0;
        return true;
    }

    /**
     * Converts a resolved value to its rendered string representation.
     * @param {any} value - The value to convert.
     * @returns {string} The rendered string, or an empty string for null and undefined values.
     */
    function convertToString(value) {
        if (value === null || value === undefined)
            return '';
        if (typeof value === 'boolean')
            return value ? 'true' : 'false';
        return String(value);
    }

    /**
     * Reads a single member from a value, matching member names case-insensitively.
     * @param {any} target - The value the member is read from.
     * @param {string} name - The member name to read.
     * @returns {any} The member value, or undefined when the member is not found.
     */
    function readMember(target, name) {
        if (target === null || target === undefined || typeof target !== 'object')
            return undefined;
        // expose the collection count under the same 'Count' member as the server side engine
        if (name.toLowerCase() === 'count' && Array.isArray(target))
            return target.length;
        // match member names case-insensitively, only against the own enumerable properties, so prototype members cannot be reached
        for (const key of Object.keys(target))
            if (key.toLowerCase() === name.toLowerCase())
                return target[key];
        return undefined;
    }

    /**
     * Resolves a dotted property path against a value, member by member.
     * @param {any} value - The value the path is resolved against.
     * @param {string} expression - The dotted property path to resolve.
     * @returns {any} The resolved value, or undefined when any part of the path is missing.
     */
    function resolvePath(value, expression) {
        let current = value;
        for (const part of expression.split('.')) {
            current = readMember(current, part);
            if (current === undefined)
                return undefined;
        }
        return current;
    }

    /**
     * Parses a template fragment into template nodes until the closing section or the end of the template.
     * @param {string} template - The template source to parse.
     * @param {number} cursor - The current position in the template, advanced as nodes are consumed.
     * @param {string|null} expectedClosingName - The name of the section that closes the fragment, or null at the top level.
     * @param {number} depth - The current section nesting depth.
     * @returns {{nodes: Array, cursor: number}} The parsed nodes and the position where the parsing stopped.
     * @throws {Error} When the template contains an invalid or unclosed tag.
     */
    function parseNodes(template, cursor, expectedClosingName, depth) {
        const nodes = [];
        while (cursor < template.length) {
            const opening = template.indexOf('{{', cursor);
            if (opening < 0) {
                if (cursor < template.length)
                    nodes.push({ type: 'text', value: template.slice(cursor) });
                cursor = template.length;
                break;
            }
            if (opening > cursor)
                nodes.push({ type: 'text', value: template.slice(cursor, opening) });

            if (template.startsWith('{{{', opening)) {
                const closing = template.indexOf('}}}', opening + 3);
                if (closing < 0)
                    throw new Error('An unescaped variable tag is not closed.');
                const expression = template.slice(opening + 3, closing).trim();
                nodes.push({ type: 'variable', expression: expression, shouldEscape: false });
                cursor = closing + 3;
                continue;
            }

            const tagClosing = template.indexOf('}}', opening + 2);
            if (tagClosing < 0)
                throw new Error('A template tag is not closed.');
            const tag = template.slice(opening + 2, tagClosing).trim();
            cursor = tagClosing + 2;
            if (tag.length === 0)
                throw new Error('Empty template tags are not allowed.');

            const first = tag[0];
            if (first === '!')
                continue;
            if (first === '#' || first === '^') {
                const expression = tag.slice(1).trim();
                const childResult = parseNodes(template, cursor, expression, depth + 1);
                nodes.push({ type: 'section', expression: expression, inverted: first === '^', children: childResult.nodes });
                cursor = childResult.cursor;
                continue;
            }
            if (first === '/') {
                const closingName = tag.slice(1).trim();
                if (expectedClosingName === null)
                    throw new Error(`Closing section '${closingName}' has no matching opening section.`);
                if (closingName !== expectedClosingName)
                    throw new Error(`Section '${expectedClosingName}' is closed by '${closingName}'.`);
                return { nodes, cursor };
            }
            if (first === '&') {
                nodes.push({ type: 'variable', expression: tag.slice(1).trim(), shouldEscape: false });
                continue;
            }
            nodes.push({ type: 'variable', expression: tag, shouldEscape: true });
        }
        if (expectedClosingName !== null)
            throw new Error(`Section '${expectedClosingName}' is not closed.`);
        return { nodes, cursor };
    }

    /**
     * Parses a template source into its nodes.
     * @param {string} template - The template source to parse.
     * @returns {Array} The parsed template nodes.
     * @throws {Error} When the template contains an invalid or unclosed tag.
     */
    function parseTemplate(template) {
        return parseNodes(template, 0, null, 0).nodes;
    }

    /**
     * Resolves a dotted expression against the scope chain, honoring explicit parent traversal.
     * @param {string} expression - The dotted property expression to resolve.
     * @param {object} startingScope - The scope the expression is resolved from, with its value and parent scope.
     * @returns {any} The resolved value, or null when no scope in the chain exposes it.
     */
    function resolveExpression(expression, startingScope) {
        let explicitParentCount = 0;
        while (expression.startsWith('../')) {
            explicitParentCount++;
            expression = expression.slice(3);
        }
        let scope = startingScope;
        for (let index = 0; index < explicitParentCount; index++)
            scope = scope.parent || scope;

        if (expression === '.')
            return scope.value;
        if (explicitParentCount > 0)
            return resolvePath(scope.value, expression);
        // resolve against the scope chain, so that a section scope can fall back to its parent values
        for (let candidate = scope; candidate !== null; candidate = candidate.parent) {
            const resolved = resolvePath(candidate.value, expression);
            if (resolved !== undefined)
                return resolved;
        }
        return null;
    }

    /**
     * Renders a list of template nodes into the output.
     * @param {Array} nodes - The template nodes to render.
     * @param {object} scope - The scope the expressions resolve against, with its value and parent scope.
     * @returns {string} The rendered text.
     */
    function renderNodes(nodes, scope) {
        let output = '';
        for (const node of nodes) {
            if (node.type === 'text') {
                output += node.value;
                continue;
            }
            if (node.type === 'variable') {
                const value = convertToString(resolveExpression(node.expression, scope));
                output += node.shouldEscape ? escapeHtml(value) : value;
                continue;
            }
            // sections are rendered when their expression is truthy, iterated when it is an array, and skipped otherwise
            const value = resolveExpression(node.expression, scope);
            const truthy = isTruthy(value);
            if (node.inverted) {
                if (!truthy)
                    output += renderNodes(node.children, scope);
                continue;
            }
            if (!truthy)
                continue;
            if (typeof value === 'boolean') {
                output += renderNodes(node.children, scope);
                continue;
            }
            if (Array.isArray(value)) {
                for (const item of value)
                    output += renderNodes(node.children, { value: item, parent: scope });
                continue;
            }
            output += renderNodes(node.children, { value: value, parent: scope });
        }
        return output;
    }

    /**
     * Renders a theme template against the provided data, using the Mustache style syntax shared with the server side theme engine.
     * @param {string} template - The template source to render.
     * @param {object} data - The data the template expressions resolve against.
     * @returns {string} The rendered text.
     */
    function renderThemeTemplate(template, data) {
        return renderNodes(parseTemplate(template), { value: data, parent: null });
    }

    window.renderThemeTemplate = renderThemeTemplate;
})();
