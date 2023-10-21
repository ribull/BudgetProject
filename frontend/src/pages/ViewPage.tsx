import React, { useState } from 'react';

import Alert from 'react-bootstrap/Alert';
import Nav from 'react-bootstrap/Nav';

type ViewChoice = 'Purchases' | 'Monthly' | 'Plan' | 'Payments'

const ViewPage: React.FC = () => {
  const [viewChoice, setViewChoice] = useState<ViewChoice>('Monthly');
  const 

  return (
    <>
      <Alert>This is the view page</Alert>
      <Nav variant="tabs">
        <Nav.Item>
          <Nav.Link eventKey="Purchases">Purchases</Nav.Link>
        </Nav.Item>
        <Nav.Item>
          <Nav.Link eventKey="Payments">Payments</Nav.Link>
        </Nav.Item>
        <Nav.Item>
          <Nav.Link eventKey="Monthly">Monthly</Nav.Link>
        </Nav.Item>
        <Nav.Item>
          <Nav.Link eventKey="Yearly">Yearly</Nav.Link>
        </Nav.Item>
      </Nav>
    </>
  );
}

export default ViewPage;
