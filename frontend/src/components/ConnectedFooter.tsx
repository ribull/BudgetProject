import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faCircleCheck,
  faTriangleExclamation,
} from '@fortawesome/free-solid-svg-icons';
import { Content, Footer } from 'react-bulma-components';

interface ConnectedFooterProps {
  isConnected: boolean;
  className: string;
}

export default function ConnectedFooter({
  isConnected,
  className,
}: ConnectedFooterProps) {
  return (
    <Footer className={className}>
      <Content>
        <h2>
          {isConnected ? 'Connected! ' : 'Not Connected! '}
          <FontAwesomeIcon
            color={isConnected ? 'green' : 'yellow'}
            icon={isConnected ? faCircleCheck : faTriangleExclamation}
          />
        </h2>
      </Content>
    </Footer>
  );
}
