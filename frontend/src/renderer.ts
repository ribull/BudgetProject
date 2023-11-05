/**
 * This file will automatically be loaded by vite and run in the "renderer" context.
 * To learn more about the differences between the "main" and the "renderer" context in
 * Electron, visit:
 *
 * https://electronjs.org/docs/tutorial/application-architecture#main-and-renderer-processes
 *
 * By default, Node.js integration in this file is disabled. When enabling Node.js integration
 * in a renderer process, please be aware of potential security implications. You can read
 * more about security risks here:
 *
 * https://electronjs.org/docs/tutorial/security
 *
 * To enable Node.js integration in this file, open up `main.ts` and enable the `nodeIntegration`
 * flag:
 *
 * ```
 *  // Create the browser window.
 *  mainWindow = new BrowserWindow({
 *    width: 800,
 *    height: 600,
 *    webPreferences: {
 *      nodeIntegration: true
 *    }
 *  });
 * ```
 */

import './index.css';

console.log('👋 This message is being logged by "renderer.ts", included via Vite');

function openNav(event: Event) {
  let targetElement: Element = event.currentTarget as Element;
  let tab: string = targetElement.id.split('-')[1];

  // Display the tab content
  let tabContents: NodeListOf<HTMLElement> = document.querySelectorAll<HTMLElement>('.nav-tab-content');
  for (let i = 0; i < tabContents.length; i++)
  {
    if (tabContents[i].id === `nav-content-${tab}`) {
      tabContents[i].style.display = 'block';
    }
    else {
      tabContents[i].style.display = 'none';
    }
  }

  // Change the tabs themselves
  let tabs: NodeListOf<HTMLElement> = document.querySelectorAll<HTMLElement>('.navbar-item');
  for (let i = 0; i < tabs.length; i++)
  {
    if (tabs[i].id == targetElement.id) {
      tabs[i].className += ' is-active';
    }
    else {
      tabs[i].className = tabs[i].className.replaceAll(' is-active', '');
    }
  }
}

let navTabs: NodeListOf<HTMLElement> = document.querySelectorAll<HTMLElement>('.navbar-item');
for (let i = 0; i < navTabs.length; i++)
{
  navTabs[i].addEventListener('click', openNav);
}