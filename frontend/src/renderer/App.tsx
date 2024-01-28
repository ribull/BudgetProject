import { MemoryRouter as Router, Routes, Route } from 'react-router-dom';
import './App.scss';
import { useEffect, useState } from 'react';
import { Button, Hero, Icon, Navbar } from 'react-bulma-components';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faGear } from '@fortawesome/free-solid-svg-icons';
import AppConstants from '../types/AppConstants';
import SettingsModal from '../components/SettingsModal';
import ConnectedFooter from '../components/ConnectedFooter';
import { ElectronHandler } from '../main/preload';
import { isApiConnected } from '../helpers/TypeSafety';

interface MainPageArgs {
  contextBridge: ElectronHandler;
}

function MainPage({ contextBridge }: MainPageArgs) {
  const [appConstants, setAppConstants] = useState<AppConstants>({
    apiUrl: null,
  });

  const [settingsWindowVisible, setSettingsWindowVisible] = useState(false);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    let timerRef: ReturnType<typeof setInterval> | undefined;

    if (appConstants.apiUrl !== undefined) {
      timerRef = setInterval(
        async () =>
          contextBridge.ipcRenderer
            .invoke('poll-online', appConstants.apiUrl)
            .then((result) => {
              if (isApiConnected(result)) {
                setIsConnected(result.connected);
                console.log(result.message);
              }

              return null;
            })
            .catch((err) => console.log(err)),
        3000,
      );
    }

    return () => clearInterval(timerRef);
  }, [contextBridge, appConstants]);

  return (
    <>
      <SettingsModal
        isVisible={settingsWindowVisible}
        settingsCallback={(settings) => {
          setSettingsWindowVisible(false);
          if (settings !== undefined) {
            setAppConstants(settings);
            contextBridge.ipcRenderer.sendMessage('settings-change', settings);
          }
        }}
      />
      <Hero size="fullheight">
        <Hero.Header>
          <Navbar style={{ flexGrow: 0 }}>
            <Navbar.Brand>
              <Navbar.Item>Budget Tool</Navbar.Item>
            </Navbar.Brand>
            <Navbar.Menu>
              <Navbar.Container>
                <Navbar.Item>Stuff</Navbar.Item>
              </Navbar.Container>
              <Navbar.Container align="right">
                <Navbar.Item>
                  <Button text={false} outlined={false}>
                    <span>
                      <Icon onClick={() => setSettingsWindowVisible(true)}>
                        <FontAwesomeIcon icon={faGear} />
                      </Icon>
                    </span>
                  </Button>
                </Navbar.Item>
              </Navbar.Container>
            </Navbar.Menu>
          </Navbar>
        </Hero.Header>
        <Hero.Body>Body!!!</Hero.Body>
        <Hero.Footer>
          <ConnectedFooter isConnected={isConnected} />
        </Hero.Footer>
      </Hero>
    </>
  );
}

export default function App({ contextBridge }: MainPageArgs) {
  const body = document.querySelector('body');
  body?.classList.add('dark');

  return (
    <Router>
      <Routes>
        <Route path="/" element={<MainPage contextBridge={contextBridge} />} />
      </Routes>
    </Router>
  );
}
