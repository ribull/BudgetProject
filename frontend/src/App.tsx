import React, { useState } from 'react';

import Container from 'react-bootstrap/Container';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import PlanPage from './pages/PlanPage';
import ViewPage from './pages/ViewPage';
import ImportPage from './pages/ImportPage';
import PredictPage from './pages/PredictPage';

type PageChoice = 'Import' | 'View' | 'Predict' | 'Plan';

const App: React.FC = () => {
  const [pageChoice, setPageChoice] = useState<PageChoice>('View');

  return (
    <div className="App" data-bs-theme="dark">
      <Navbar expand="lg" className="bg-body-tertiary">
        <Container>
          <Navbar.Brand href="#home">React-Bootstrap</Navbar.Brand>
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse id="basic-navbar-nav">
            <Nav fill className="me-auto" variant="tabs" onSelect={(eventKey) => setPageChoice(eventKey as PageChoice)}>
              <Nav.Item>
                <Nav.Link eventKey="View">View</Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="Import">Import</Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="Plan">Plan</Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="Predict">Predict</Nav.Link>
              </Nav.Item>
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
      {pageChoice === 'Import' && <ImportPage />}
      {pageChoice === 'View' && <ViewPage />}
      {pageChoice === 'Predict' && <PredictPage />}
      {pageChoice === 'Plan' && <PlanPage />}
    </div>
  );
}

export default App;
